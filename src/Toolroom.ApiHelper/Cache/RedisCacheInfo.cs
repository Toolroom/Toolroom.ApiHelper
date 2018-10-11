using System;

namespace Toolroom.ApiHelper
{
    public class RedisCacheInfo
    {
        internal RedisCacheInfo()
        {
        }
        public bool IsConnected { get; set; }
        public long UsedMemory { get; set; }
        public long MaximumMemory { get; set; }
        public double? MemoryPercentage
        {
            get
            {
                if (MaximumMemory <= 0)
                {
                    return null;
                }
                return Math.Round(100.0 * UsedMemory / MaximumMemory, 2);
            }
        }
    }
}