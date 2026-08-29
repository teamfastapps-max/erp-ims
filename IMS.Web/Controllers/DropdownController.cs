using Microsoft.AspNetCore.Mvc;
using IMS.Models.Common.Dropdown;
using IMS.Services.Interfaces;

namespace IMS.Controllers
{
    /// <summary>
    /// Generic Dropdown Controller
    /// Used by all dropdowns throughout the application.
    /// </summary>
    public class DropdownController : Controller
    {
        private readonly IDropdownService _dropdownService;

        public DropdownController(IDropdownService dropdownService)
        {
            _dropdownService = dropdownService;
        }

        /// <summary>
        /// Returns dropdown data.
        ///
        /// Examples:
        ///
        /// /Dropdown/GetDropdown?entityType=PaymentMode
        ///
        /// /Dropdown/GetDropdown?entityType=ProductCategory
        ///
        /// /Dropdown/GetDropdown?entityType=District&parentId=5
        ///
        /// /Dropdown/GetDropdown?entityType=Product&search=laptop
        /// </summary>
        [HttpGet]
        public JsonResult GetDropdown(
            string entityType,
            int? parentId = null,
            string search = null,
            bool activeOnly = true,
            int page = 1,
            int pageSize = 100)
        {
            try
            {
                var request = new DropdownRequestModel
                {
                    EntityType = entityType,
                    ParentId = parentId,
                    Search = search,
                    ActiveOnly = activeOnly,
                    Page = page,
                    PageSize = pageSize
                };

                var data = _dropdownService.GetDropdown(request);

                return Json(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;

                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}