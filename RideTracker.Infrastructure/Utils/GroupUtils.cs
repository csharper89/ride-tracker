using RideTracker.Infrastructure.DbModels;
using SQLite;

namespace RideTracker.Utilities;

public class GroupUtils(ISQLiteAsyncConnection db)
{
    private Group? _currentGroup;

    public async virtual Task<Guid?> GetCurrentGroupIdAsync()
    {
        if(_currentGroup is null)
        {
            _currentGroup = await db.Table<Group>().FirstOrDefaultAsync(x => x.IsCurrent && x.DeletedAt == null);      
        }

        return _currentGroup?.Id;
    }

    public async virtual Task<bool> IsUserManagingCurrentGroupAsync()
    {
        if (_currentGroup is null)
        {
            _currentGroup = await db.Table<Group>().FirstOrDefaultAsync(x => x.IsCurrent && x.DeletedAt == null);
        }

        return _currentGroup?.IsManagedByCurrentUser ?? false;
    }

    public async virtual Task<bool> IsCurrentGroupSetAsync()
    {
        var currentGroup = await db.Table<Group>().FirstOrDefaultAsync(x => x.IsCurrent && x.DeletedAt == null);
        return currentGroup is not null;
    }

    public void ResetCurrentGroup()
    {
        _currentGroup = null;
    }
}
