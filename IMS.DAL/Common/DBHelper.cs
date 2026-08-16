using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL.Common
{
    public class DBHelper
    {
        private readonly string _connectionString;

        public DBHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlCommand CreateCommand(string spName, SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }
    }
}
