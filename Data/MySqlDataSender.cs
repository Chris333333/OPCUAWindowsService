using Data.DTOs;
using Data.Extensions;
using Data.Scaffolded;
using Opc.Ua;
using Serilog;

namespace Data
{
    public class MySqlDataSender : IDisposable
    {
        private readonly DataBaseContext _context;

        public MySqlDataSender()
        {
            _context = new DataBaseContext();
        }

        
        public void MySQLUpdateOneLayoutInDatabase(OPCUATag opcuaTag)
        {
            var error = MySQLDataSenderExtension.UpdateOneLayoutInDatabase(opcuaTag, _context);
            if (error.IsError && error.Message != null)
                Log.Error(error.Message);

        }

        public void MySQLUpdateWholeTableInDatabase(Dictionary<NodeId, OPCUATag> values)
        {
            var error = MySQLDataSenderExtension.UpdateWholeTableInDatabase(values, _context);
            if (error.IsError && error.Message != null)
                Log.Error(error.Message);
        }

        public void MySQLRemadeTableInDatabase(Dictionary<NodeId, OPCUATag> values)
        {
            var error = MySQLDataSenderExtension.RemedeWholeTableInDatabase(values, _context);
            if (error.IsError && error.Message != null)
                Log.Error(error.Message);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}