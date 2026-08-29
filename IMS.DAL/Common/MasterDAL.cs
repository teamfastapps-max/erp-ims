// IMS.DAL/Common/MasterDAL.cs
using System.Data;
using IMS.DAL.Interfaces;
using IMS.Models.Common.Master;

namespace IMS.DAL.Common
{
    /// <summary>
    /// Generic DAL for all master/lookup tables. Each entity has ONE
    /// multi-action stored procedure (USP_{EntityName}) that internally
    /// handles GetAll/GetById/Insert/Update/Deactivate/Delete/ExistsByField
    /// with transaction + rollback safety for write actions.
    /// </summary>
    public class MasterDAL : IMasterDAL
    {
        private readonly DBHelper _dbHelper;

        public MasterDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Dictionary<string, object>> GetAll(MasterConfig config)
        
        {
            var parameters = new Dictionary<string, object> { { "@Action", "GetAll" } };
            var table = _dbHelper.ExecuteStoredProcedure(config.SpName, parameters);
            return ToDictionaryList(table);
        }

        public Dictionary<string, object> GetById(MasterConfig config, Guid id)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Action", "GetById" },
                { "@" + config.KeyColumn, id }      // was: { "@Id", id }
            };

            var table = _dbHelper.ExecuteStoredProcedure(config.SpName, parameters);
            return ToDictionaryList(table).FirstOrDefault();
        }

        public bool Update(MasterConfig config, Guid id, Dictionary<string, object> values, Guid tenantId, Guid updatedBy)
        {
            var parameters = BuildFieldParameters(config, values);

            parameters["@Action"] = "Update";
            parameters["@" + config.KeyColumn] = id;

            if (config.HasAuditColumns)
            {
                parameters["@TenantId"] = tenantId;
                parameters["@UpdatedBy"] = updatedBy;
            }

            return _dbHelper.ExecuteStoredProcedureNonQuery(
                config.SpName,
                parameters) != 0;
        }

        public bool Delete(MasterConfig config, Guid id)
        {
            var parameters = new Dictionary<string, object>
        {
            { "@Action", config.SoftDelete ? "Deactivate" : "Delete" },
            { "@" + config.KeyColumn, id }
            //{ "@ModifiedBy", modifiedBy }
        };

            var rows = _dbHelper.ExecuteStoredProcedureNonQuery(config.SpName, parameters);
            return rows != 0;   // same fix
        }

        public Guid Insert(MasterConfig config, Dictionary<string, object> values, Guid tenantId, Guid createdBy)
        {
            var parameters = BuildFieldParameters(config, values);

            parameters["@Action"] = "Insert";
            parameters["@NewId"] = DBNull.Value;

            if (config.HasAuditColumns)
            {
                parameters["@TenantId"] = tenantId;
                parameters["@CreatedBy"] = createdBy;
            }

            var outputVal = _dbHelper.ExecuteStoredProcedureWithOutput(
                config.SpName,
                parameters,
                "@NewId");

            return Guid.TryParse(outputVal?.ToString(), out var newGuid) ? newGuid : Guid.Empty;
        }

        public bool ExistsByField(MasterConfig config, string columnName, object value, Guid? excludeId = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Action", "ExistsByField" },
                { "@ColumnName", columnName },
                { "@Value", value },
                { "@ExcludeId", excludeId.HasValue ? (object)excludeId.Value : DBNull.Value }
            };

            var result = _dbHelper.ExecuteStoredProcedureScalar(config.SpName, parameters);
            return Convert.ToInt32(result) > 0;
        }

        // ---------- Private helpers ----------

        private Dictionary<string, object> BuildFieldParameters(MasterConfig config, Dictionary<string, object> values)
        {
            var parameters = new Dictionary<string, object>();

            foreach (var field in config.Fields)
            {
                if (!values.ContainsKey(field.ColumnName)) continue;

                var rawValue = values[field.ColumnName];
                var convertedValue = ConvertValueForFieldType(field, rawValue);
                parameters["@" + field.ColumnName] = convertedValue;
            }

            return parameters;
        }

        /// <summary>
        /// Converts a raw value (typically from JSON) to the correct .NET type
        /// based on MasterFieldType. This ensures DBHelper.AddParameters creates
        /// the correct SqlDbType for SQL Server.
        /// </summary>
        private object ConvertValueForFieldType(MasterFieldConfig field, object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
                return DBNull.Value;

            var stringValue = rawValue.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return DBNull.Value;

            switch (field.FieldType)
            {
                case MasterFieldType.Date: 
                case MasterFieldType.DateTime:
                    if (DateTime.TryParse(stringValue,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var dateVal))
                    {
                        return dateVal;
                    }
                    return DBNull.Value;

                case MasterFieldType.Number:
                    if (decimal.TryParse(stringValue, out var numVal))
                        return numVal;
                    return rawValue;

                case MasterFieldType.Boolean:
                    if (bool.TryParse(stringValue, out var boolVal))
                        return boolVal;
                    if (stringValue == "1") return true;
                    if (stringValue == "0") return false;
                    return rawValue;

                default:
                    return rawValue;
            }
        }

        private List<Dictionary<string, object>> ToDictionaryList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                list.Add(dict);
            }
            return list;
        }
    }
}