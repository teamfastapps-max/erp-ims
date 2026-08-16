using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using IMS.DAL.Common;

namespace IMS.Web.Controllers
{
    public class DbTestController : Controller
    {
        private readonly DBHelper _dbHelper;

        public DbTestController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = new List<VendorTestItem>();

            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT V_Id, V_TenantId, V_VendorCode, V_VendorName,
                       V_TaxRegistrationNumber, V_IsActive, V_CreatedDate, V_ModifiedDate
                FROM Vendors_V
                ORDER BY V_Id DESC", con);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new VendorTestItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("V_Id")),
                    TenantId = reader.GetGuid(reader.GetOrdinal("V_TenantId")),
                    VendorCode = reader.GetString(reader.GetOrdinal("V_VendorCode")),
                    VendorName = reader.GetString(reader.GetOrdinal("V_VendorName")),
                    TaxNumber = reader.IsDBNull(reader.GetOrdinal("V_TaxRegistrationNumber"))
                        ? null : reader.GetString(reader.GetOrdinal("V_TaxRegistrationNumber")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("V_IsActive")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("V_CreatedDate")),
                    ModifiedDate = reader.IsDBNull(reader.GetOrdinal("V_ModifiedDate"))
                        ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("V_ModifiedDate"))
                });
            }

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string vendorCode, string vendorName, string taxNumber)
        {
            if (string.IsNullOrWhiteSpace(vendorCode) || string.IsNullOrWhiteSpace(vendorName))
            {
                TempData["Error"] = "Vendor code and name are required.";
                return RedirectToAction(nameof(Index));
            }

            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Vendors_V (V_TenantId, V_VendorCode, V_VendorName, V_TaxRegistrationNumber,
                                       V_IsActive, V_CreatedBy, V_CreatedDate)
                VALUES (@tid, @code, @name, @tax, 1, 1, GETDATE())", con);

            cmd.Parameters.AddWithValue("@tid", Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
            cmd.Parameters.AddWithValue("@code", vendorCode);
            cmd.Parameters.AddWithValue("@name", vendorName);
            cmd.Parameters.AddWithValue("@tax", (object)taxNumber ?? DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            TempData["Success"] = "Vendor created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("DELETE FROM Vendors_V WHERE V_Id = @id", con);
            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            TempData["Success"] = "Vendor deleted.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class VendorTestItem
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string TaxNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
