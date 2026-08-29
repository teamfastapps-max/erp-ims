// IMS.Web/Controllers/MasterController.cs
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using IMS.Helpers.Constants;
using IMS.Models.Common.Master;
using IMS.Services.Interfaces;
using IMS.Web.Models;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Generic controller serving all master/lookup entities.
    /// Route: /Master/{entityType}  e.g. /Master/Bank, /Master/Warehouse
    /// One controller + one view handles all 9 master tables.
    /// </summary>
    [Route("Master")]
    public class MasterController : Controller
    {
        private readonly IMasterService _masterService;

        public MasterController(IMasterService masterService)
        {
            _masterService = masterService;
        }

        private Guid CurrentTenantId
        {
            get
            {
                var raw = User.FindFirst("tenant_id")?.Value;
                return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
            }
        }

        private Guid CurrentUserId
        {
            get
            {
                var raw = User.FindFirst("user_id")?.Value;
                return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
            }
        }

        // Add this action to MasterController.cs
        [HttpGet("")]
        public IActionResult Menu()
        {
            var allConfigs = MasterConfigRegistry.GetAll();
            return View("Menu", allConfigs);
        }

        // GET: /Master/{entityType}
        [HttpGet("{entityType}")]
        public IActionResult Index(string entityType)
        {
            var config = MasterConfigRegistry.GetByEntityType(entityType);
            if (config == null)
                return NotFound();

            // TODO: permission check using config.ViewPermission against your Authorization policy

            ViewBag.Config = config;
            return View(config);
        }

        // GET: /Master/{entityType}/List  (AJAX - grid data)
        [HttpGet("{entityType}/List")]
        public IActionResult List(string entityType)
        {
            try
            {
                var data = _masterService.GetAll(entityType);
                return Json(new { success = true, data });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // GET: /Master/{entityType}/{id}
        [HttpGet("{entityType}/{id:guid}")]
        public IActionResult GetById(string entityType, Guid id)
        {
            try
            {
                var record = _masterService.GetById(entityType, id);
                if (record == null)
                    return NotFound(new { success = false, message = "Record not found." });

                return Json(new { success = true, data = record });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // POST: /Master/{entityType}
        [HttpPost("{entityType}")]
        public IActionResult Create(string entityType, [FromBody] MasterFormViewModel model)
        {
            if (model?.Values == null)
                return BadRequest(new { success = false, message = "Invalid form data." });


            var result = _masterService.Create(entityType, model.Values, CurrentTenantId, CurrentUserId);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message, id = result.Id });
        }

        // PUT: /Master/{entityType}/{id}
        [HttpPut("{entityType}/{id:guid}")]
        public IActionResult Update(string entityType, Guid id, [FromBody] MasterFormViewModel model)
        {
            if (model?.Values == null)
                return BadRequest(new { success = false, message = "Invalid form data." });


            var result = _masterService.Update(entityType, id, model.Values, CurrentTenantId, CurrentUserId);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }

        // DELETE: /Master/{entityType}/{id}
        [HttpDelete("{entityType}/{id:guid}")]
        public IActionResult Delete(string entityType, Guid id)
        {

            var result = _masterService.Delete(entityType, id);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }

        // ---------- Helpers ----------
    }
}