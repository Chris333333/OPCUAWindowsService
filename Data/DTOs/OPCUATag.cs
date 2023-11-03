namespace Data.DTOs
{
    public class OPCUATag
    {
        public OPCUATag(string displayName,string nodeID)
        {
            NodeID = nodeID;
            DisplayName = displayName;
        }

        public OPCUATag(string displayName, string nodeID, DateTime lastUpdatedTime, DateTime lastSourceTimeStamp, string? lastGoodValue = null, string? currentValue = null, string? statusCode= null) : this(displayName, nodeID)
        {
            LastUpdatedTime = lastUpdatedTime;
            LastSourceTimeStamp = lastSourceTimeStamp;
            LastGoodValue = lastGoodValue;
            CurrentValue = currentValue;
            StatusCode = statusCode;
        }

        public string NodeID { get; set; }
        public string DisplayName { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public DateTime LastSourceTimeStamp { get; set;}
        public string? LastGoodValue { get; set; }
        public string? CurrentValue { get; set;}
        public string? StatusCode { get; set;}
    }
}
