using Data.DTOs;
using Data.Scaffolded;
using System.Text;
using Opc.Ua;
using Microsoft.EntityFrameworkCore;
using Domain.Scaffolded;
using Serilog;

namespace Data.Extensions
{
    public static class MySQLDataSenderExtension
    {
        [Obsolete("Too much data for sending one at the time")]
        public static ErrorLogDTO UpdateOneLayoutInDatabase(OPCUATag opcuaTag, DataBaseContext dbContext)
        {
            if(dbContext == null)
                return new ErrorLogDTO(true,"DbContext is null when function to update one layout is called");
            if (opcuaTag == null)
                return new ErrorLogDTO(true, "OPCUATag is null when function to update one layout is called");

            if(opcuaTag.NodeID == null)
                return new ErrorLogDTO(true, "NodeID is null when function to update one layout is called");
            if (opcuaTag.DisplayName == null)
                return new ErrorLogDTO(true, "DisplayName is null when function to update one layout is called");

            var result = dbContext.OpcuaData.Single(x => x.NodeId == opcuaTag.NodeID);
            if (result == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("There is no NodeId: ");
                sb.Append(opcuaTag.NodeID.ToString());
                sb.Append(" in the database.");
                return new ErrorLogDTO(true, sb.ToString());
            }

            result.CurrentValue = opcuaTag.CurrentValue;
            result.LastGoodValue = opcuaTag.LastGoodValue;
            result.LastUpdatedTime = opcuaTag.LastUpdatedTime;
            result.LastSourceTimeStamp = opcuaTag.LastSourceTimeStamp;
            result.StatusCode = opcuaTag.StatusCode;
            dbContext.SaveChanges();

            return new ErrorLogDTO(false);
        }

        public static ErrorLogDTO UpdateWholeTableInDatabase(Dictionary<NodeId, OPCUATag> values, DataBaseContext dbContext)
        {
            if (dbContext == null)
                return new ErrorLogDTO(true, "DbContext is null when function to update one layout is called");
            if (values == null)
                return new ErrorLogDTO(true, "values are null when function to update one layout is called");
            
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = TimeSpan.FromSeconds(2);


            foreach (KeyValuePair< NodeId, OPCUATag > x in values)
            {
                if ((now - x.Value.LastUpdatedTime) <= timeSpan)
                {
                    var result = dbContext.OpcuaData.FirstOrDefault(y => y.NodeId == x.Key.ToString()); // TO STRING
                    if (result != null)
                    {
                        result.CurrentValue = x.Value.CurrentValue;
                        result.LastGoodValue = x.Value.LastGoodValue;
                        result.LastUpdatedTime = x.Value.LastUpdatedTime;
                        result.LastSourceTimeStamp = x.Value.LastSourceTimeStamp;
                        result.StatusCode = x.Value.StatusCode;
                    }
                }

            }
            dbContext.SaveChanges();

            //Log.Information("Send data to database");
            return new ErrorLogDTO(false);
        }

        [Obsolete("Hardcoded to clear table opcua_data")]
        public static ErrorLogDTO RemedeWholeTableInDatabase(Dictionary<NodeId, OPCUATag> values, DataBaseContext dbContext)
        {
            if (dbContext == null)
                return new ErrorLogDTO(true, "DbContext is null when function to update one layout is called");
            if (values == null)
                return new ErrorLogDTO(true, "values are null when function to update one layout is called");

            dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE opcua_data");

            DateTime now = DateTime.Now;

            foreach (KeyValuePair<NodeId, OPCUATag> x in values)
            {
                var data = new OpcuaDatum()
                {
                    LastGoodValue = x.Value.LastGoodValue,
                    CurrentValue = x.Value.CurrentValue,
                    LastUpdatedTime = x.Value.LastUpdatedTime,
                    LastSourceTimeStamp = x.Value.LastSourceTimeStamp,
                    StatusCode = x.Value.StatusCode,
                    NodeId = x.Key.ToString(),
                    DisplayName = x.Value.DisplayName,
                };

                dbContext.Add(data);
            }
            dbContext.SaveChanges();

            return new ErrorLogDTO(false);
        }
    }
}
