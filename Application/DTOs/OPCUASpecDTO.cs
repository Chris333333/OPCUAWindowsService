using Data.DTOs;
using Opc.Ua;
using Serilog;

namespace Application.DTOs
{
    public record OPCUASpecDTO(string ServerAddres, string ServerPort, Dictionary<NodeId, OPCUATag> TagList, bool SessionRenewalRequired, double SessionRenewalMinutes);
}
