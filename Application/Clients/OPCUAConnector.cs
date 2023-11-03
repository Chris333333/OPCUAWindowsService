using Application.DTOs;
using Opc.Ua.Client;
using Serilog;
using System.Text;

namespace Application.Clients
{
    /// <summary>
    /// Child class of OPCUAClient with own implemetation of what occurs when event OnTagValueChange is triggered
    /// </summary>
    public class OPCUAConnector : OPCUAClient
    {
        public OPCUAConnector(OPCUASpecDTO opcuaSpec) : base(opcuaSpec) {}

        protected override void OnTagValueChange(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                foreach (var value in item.DequeueValues())
                {

                    if (item.DisplayName == "ServerStatusCurrentTime")
                    {
                        LastTimeOPCServerFoundAlive = value.SourceTimestamp.ToLocalTime();

                    }
                    else
                    {
                        if (TagList.ContainsKey(item.StartNodeId))
                        {
                            if (TagList[item.StartNodeId] != null)
                            {
                                if (value.Value != null)
                                {
                                    if (value.Value.GetType() == typeof(bool[]))
                                    {
                                        TagList[item.StartNodeId].CurrentValue = ((Array)value.Value).GetValue(2).ToString();
                                        TagList[item.StartNodeId].LastGoodValue = ((Array)value.Value).GetValue(2).ToString();
                                    }
                                    else if (value.Value.GetType() == typeof(bool) || value.Value.GetType() == typeof(byte) || value.Value.GetType() == typeof(int))
                                    {
                                        TagList[item.StartNodeId].CurrentValue = value.Value.ToString();
                                        TagList[item.StartNodeId].LastGoodValue = value.Value.ToString();
                                    }
                                    else if (value.Value.GetType() == typeof(string[]))
                                    {
                                        TagList[item.StartNodeId].CurrentValue = ((Array)value.Value).GetValue(0).ToString();
                                        TagList[item.StartNodeId].LastGoodValue = ((Array)value.Value).GetValue(0).ToString();
                                    }
                                    else
                                    {
                                        TagList[item.StartNodeId].CurrentValue = "Not supported data type";
                                        TagList[item.StartNodeId].LastGoodValue = "Not supported data type";

                                    }
                                }
                                else
                                {
                                    TagList[item.StartNodeId].CurrentValue = "No data";
                                    TagList[item.StartNodeId].LastGoodValue = "No data";

                                }

                                TagList[item.StartNodeId].LastUpdatedTime = DateTime.Now;
                                TagList[item.StartNodeId].LastSourceTimeStamp = value.SourceTimestamp.ToLocalTime();
                                TagList[item.StartNodeId].StatusCode = value.StatusCode.ToString();

                                //Log.Information(item.StartNodeId.ToString());
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: OnTagValueChange: exception when handling tag value change: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                Log.Error(sb.ToString());
            }

        }
    }
}
