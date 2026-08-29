using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.Json;
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

         // ---------- Stored Procedure execution helpers ----------
        public DataTable ExecuteStoredProcedure(string spName, Dictionary<string, object> parameters)
        {
            using (var connection = GetConnection())
            using (var cmd = CreateCommand(spName, connection))
            {
                AddParameters(cmd, parameters);

                var table = new DataTable();
                connection.Open();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
                return table;
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns a single scalar value.
        /// Use for ExistsByField / COUNT-style checks.
        /// </summary>
        public object ExecuteStoredProcedureScalar(string spName, Dictionary<string, object> parameters)
        {
            using (var connection = GetConnection())
            using (var cmd = CreateCommand(spName, connection))
            {
                AddParameters(cmd, parameters);

                connection.Open();
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes a stored procedure with no result set expected.
        /// Returns rows affected. Use for Update / Delete / Deactivate.
        /// </summary>
        public int ExecuteStoredProcedureNonQuery(string spName, Dictionary<string, object> parameters)
        {
            using (var connection = GetConnection())
            using (var cmd = CreateCommand(spName, connection))
            {
                AddParameters(cmd, parameters);

                connection.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a stored procedure that returns a value via an OUTPUT parameter
        /// (e.g. @NewId for Insert). Returns the output parameter's value.
        /// </summary>
        public object ExecuteStoredProcedureWithOutput(string spName, Dictionary<string, object> parameters, string outputParamName, SqlDbType outputType = SqlDbType.UniqueIdentifier)
        {
            using (var connection = GetConnection())
            using (var cmd = CreateCommand(spName, connection))
            {
                AddParameters(cmd, parameters);

                // Mark the output parameter correctly (it was added as a normal Input param above)
                if (cmd.Parameters.Contains(outputParamName))
                {
                    cmd.Parameters[outputParamName].Direction = ParameterDirection.Output;
                    cmd.Parameters[outputParamName].SqlDbType = outputType;
                }

                connection.Open();
                cmd.ExecuteNonQuery();

                var value = cmd.Parameters[outputParamName].Value;
                return value == DBNull.Value ? null : value;
            }
        }

        private void AddParameters(SqlCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;

            foreach (var kvp in parameters)
            {
                object value = UnwrapValue(kvp.Value);

                SqlParameter p = new SqlParameter();
                p.ParameterName = kvp.Key;
                p.Value = value ?? DBNull.Value;

                switch (value)
                {
                    case int:
                        p.SqlDbType = SqlDbType.Int;
                        break;

                    case bool:
                        p.SqlDbType = SqlDbType.Bit;
                        break;

                    case Guid:
                        p.SqlDbType = SqlDbType.UniqueIdentifier;
                        break;

                    case decimal:
                        p.SqlDbType = SqlDbType.Decimal;
                        break;

                    case DateTime:
                        p.SqlDbType = SqlDbType.DateTime2;
                        break;

                    default:
                        p.SqlDbType = SqlDbType.NVarChar;
                        break;
                }

                cmd.Parameters.Add(p);
            }
        }

        private object UnwrapValue(object value)
        {
            if (value is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:

                        string s = je.GetString();

                        if (string.IsNullOrWhiteSpace(s))
                            return DBNull.Value;

                        if (int.TryParse(s, out int i))
                            return i;

                        if (decimal.TryParse(s, out decimal d))
                            return d;

                        if (Guid.TryParse(s, out Guid g))
                            return g;

                        if (bool.TryParse(s, out bool b))
                            return b;

                        // Parse date strings in yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss formats
                        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime dt))
                            return dt;

                        return s;

                    case JsonValueKind.Number:

                        if (je.TryGetInt32(out int intVal))
                            return intVal;

                        if (je.TryGetInt64(out long longVal))
                            return longVal;

                        if (je.TryGetDecimal(out decimal decVal))
                            return decVal;

                        return je.GetDouble();

                    case JsonValueKind.True:
                        return true;

                    case JsonValueKind.False:
                        return false;

                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        return DBNull.Value;

                    default:
                        return je.ToString();
                }
            }

            return value;
        }

        public async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
        
        public SqlCommand CreateStoredProcCommand(SqlConnection conn, string procedureName, SqlTransaction tx = null)
        {
            var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (tx != null) cmd.Transaction = tx;
            return cmd;
        }

        public static SqlParameter Param(string name, object value, SqlDbType? type = null)
        {
            var p = new SqlParameter(name, value ?? DBNull.Value);
            if (type.HasValue) p.SqlDbType = type.Value;
            return p;
        }
        
        public static SqlParameter TableParam(string name, DataTable table, string sqlTypeName)
        {
            return new SqlParameter(name, SqlDbType.Structured)
            {
                TypeName = sqlTypeName,
                Value = table
            };
        }
    }
}
