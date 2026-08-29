using System;
using System.Collections.Generic;
using System.Data;
using IMS.DAL.Interfaces;
using IMS.Models.Common;
using IMS.Models.Common.Dropdown;

namespace IMS.DAL.Common
{
    /// <summary>
    /// Generic Dropdown DAL
    /// Handles all dropdowns using a single stored procedure.
    /// </summary>
    public class DropdownDAL : IDropdownDAL
    {
        private readonly DBHelper _dbHelper;

        public DropdownDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Returns dropdown data for any registered entity.
        /// </summary>
        public List<DropdownItemModel> GetDropdown(DropdownConfig config, DropdownRequestModel request)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var parameters = new Dictionary<string, object>
            {
                { "@TableName", config.TableName },
                { "@KeyColumn", config.KeyColumn },
                { "@ValueColumn", config.ValueColumn },
                { "@TextColumn", config.TextColumn },
                { "@CodeColumn", config.CodeColumn ?? (object)DBNull.Value },
                { "@ActiveColumn", config.ActiveColumn ?? (object)DBNull.Value },
                { "@ParentColumn", config.ParentColumn ?? (object)DBNull.Value },
                { "@ParentId", request.ParentId ?? (object)DBNull.Value },
                { "@Search", string.IsNullOrWhiteSpace(request.Search) ? (object)DBNull.Value : request.Search },
                { "@ActiveOnly", request.ActiveOnly },
                { "@OrderByColumn", config.OrderByColumn },
                { "@OrderByDirection", config.OrderByDirection }
            };

            DataTable dt = _dbHelper.ExecuteStoredProcedure(
                "USP_GenericDropdown",
                parameters);

            return ConvertToDropdownItems(dt);
        }

        /// <summary>
        /// Converts DataTable into DropdownItemModel list.
        /// </summary>
        private List<DropdownItemModel> ConvertToDropdownItems(DataTable dt)
        {
            var list = new List<DropdownItemModel>();

            if (dt == null || dt.Rows.Count == 0)
                return list;

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new DropdownItemModel
                {
                    Value = Convert.ToInt32(row["Value"]),
                    Text = row["Text"]?.ToString(),
                    Code = row.Table.Columns.Contains("Code")
                                ? row["Code"]?.ToString()
                                : null,

                    ParentId = row.Table.Columns.Contains("ParentId")
                                && row["ParentId"] != DBNull.Value
                                ? Convert.ToInt32(row["ParentId"])
                                : (int?)null,

                    IsActive = row.Table.Columns.Contains("IsActive")
                                && row["IsActive"] != DBNull.Value
                                ? Convert.ToBoolean(row["IsActive"])
                                : true
                });
            }

            return list;
        }
    }
}