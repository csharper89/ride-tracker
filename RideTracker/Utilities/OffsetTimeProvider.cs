using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideTracker.Utilities
{
    public class OffsetTimeProvider : TimeProvider
    {
        private readonly TimeSpan _offset = TimeSpan.FromDays(-2);

        public override DateTimeOffset GetUtcNow() => base.GetUtcNow().Add(_offset);

        public override long GetTimestamp() => base.GetTimestamp();
    }
}
