using RideTracker.Infrastructure.DbModels.Interfaces;
using RideTracker.Utilities;
using SQLite;

namespace RideTracker.Infrastructure.Synchronization;

public abstract class EntitySynchronizer<T, TResponse>
    where T : IBaseEntity, IModifiableEntity, new()
    where TResponse : ApiResponseBase
{
    protected readonly RideTrackerHttpClient _httpClient;
    protected readonly ISQLiteAsyncConnection _db;
    protected readonly DbLogger<EntitySynchronizer<T, TResponse>> _logger;
    protected readonly TimeProvider _timeProvider;
    protected readonly GroupUtils _groupUtils;

    public EntitySynchronizer(RideTrackerHttpClient httpClient, ISQLiteAsyncConnection db, DbLogger<EntitySynchronizer<T, TResponse>> logger, TimeProvider timeProvider, GroupUtils groupUtils)
    {
        _httpClient = httpClient;
        _db = db;
        _logger = logger;
        _timeProvider = timeProvider;
        _groupUtils = groupUtils;
    }

    public async Task UploadEntitiesToCloudAsync()
    {
        try
        {            
            await UploadEntitiesToCloudInternalAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading entities to cloud");
        }
    }

    private async Task UploadEntitiesToCloudInternalAsync()
    {
        _logger.LogInformation($"Starting UploadEntitiesToCloudAsync. T is {typeof(T)}");
        var fifteenMinutesAgo = _timeProvider.GetUtcNow().AddMinutes(-15);
        if (!await CanUploadAsync())
        {
            _logger.LogInformation($"Can't upload. Table: {typeof(T)}");
            return;
        }

        var entitiesToUpload = await _db.Table<T>()
            .Where(e => e.IsUploadedToCloud == false && e.CreatedAt < fifteenMinutesAgo)
            .ToListAsync();

        _logger.LogInformation($"Found {entitiesToUpload.Count} entities to upload");

        foreach (var entity in entitiesToUpload)
        {
            await UploadSingleEntityToCloudAsync(entity);
        }
        _logger.LogInformation("Finished UploadEntitiesToCloudAsync");
    }

    public async Task FetchEntitiesFromCloudAsync()
    {        
        if(!await CanFetchAsync())
        {
            _logger.LogInformation($"Can't fetch. Table: {typeof(T)}");
            return;
        }

        var lastSynchronizationTime = await GetLastSynchronizationTimeAsync();
        _logger.LogInformation($"Starting FetchEntitiesFromCloudAsync. Last synchronization time: {lastSynchronizationTime}");
        try
        {
            var endpoint = await GetFetchEndpointAsync();
            var response = await _httpClient.GetAsync<List<TResponse>>($"{endpoint}?startFrom={lastSynchronizationTime:o}");
            if (response is null || response.Count == 0)
            {
                _logger.LogInformation("No entities found to fetch from cloud");
            }

            _logger.LogInformation($"Fetched {response.Count} entities from cloud");
            foreach (var entityResponse in response)
            {
                await ProcessEntityResponseAsync(entityResponse);
            }

            OnFetchCompleted(response.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entities from cloud");
        }
    }

    public async Task UploadSingleEntityToCloudAsync(T entity)
    {
        _logger.LogInformation($"Uploading entity with id {entity.Id}");
        await MarkForUploadAsync(entity);
        var request = GetUploadRequest(entity);

        try
        {
            var endpoint = await GetUploadEndpointAsync(entity);
            var response = await _httpClient.PostAsync<SynchronizationTimeResponse>(endpoint, request);
            if (response is not null)
            {
                entity.IsUploadedToCloud = true;
                await _db.UpdateAsync(entity);
                _logger.LogInformation($"Successfully uploaded vehicle {entity.Id}");
            }
            else
            {
                _logger.LogError($"Response was null when uploading vehicle {entity.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading vehicle {entity.Id}");
        }
    }

    private async Task MarkForUploadAsync(T entity)
    {
        if (entity.IsUploadedToCloud)
        {
            entity.IsUploadedToCloud = false;
            await _db.UpdateAsync(entity);
        }
    }
    protected async Task ProcessEntityResponseAsync(TResponse entityResponse)
    {
        _logger.LogInformation($"Processing response entity with id {entityResponse.Id}");
        var existingEntity = await _db.Table<T>().FirstOrDefaultAsync(v => v.Id == entityResponse.Id);

        if (existingEntity is not null)
        {
            await UpdateExistingEntityAsync(entityResponse, existingEntity);
        }
        else
        {
            await CreateNewEntityAsync(entityResponse);
        }
    }

    protected async Task UpdateExistingEntityAsync(TResponse response, T existingEntity)
    {
        _logger.LogInformation($"Updating existing vehicle: {existingEntity.Id}");
        UpdateEntityFromResponse(response, existingEntity);

        await _db.UpdateAsync(existingEntity);
        _logger.LogInformation($"Successfully updated entity with id: {existingEntity.Id}");
    }

    protected async Task CreateNewEntityAsync(TResponse response)
    {
        _logger.LogInformation($"Creating new entity: {response.Id}");
        var newEntity = CreateEntityFromResponse(response);
        newEntity.IsUploadedToCloud = true;

        await _db.InsertAsync(newEntity);
        _logger.LogInformation($"Successfully created vehicle: {newEntity.Id}");
    }

    protected virtual void OnFetchCompleted(int fetchedCount)  {  }
    protected abstract object GetUploadRequest(T entity);
    protected abstract T CreateEntityFromResponse(TResponse response);
    protected abstract void UpdateEntityFromResponse(TResponse response, T existingEntity);
    protected abstract Task<string> GetFetchEndpointAsync();
    protected abstract Task<string> GetUploadEndpointAsync(T entity);
    protected abstract Task<bool> CanUploadAsync();
    protected abstract Task<bool> CanFetchAsync();
    protected abstract Task<DateTime> GetLastSynchronizationTimeAsync();
}