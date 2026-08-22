using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace IMS.Helpers.Constants
{
    /// <summary>
    /// TEMPORARY: Stand-in for the Master Data module (Organizations, Branches).
    /// Replace with real DB-backed lookups once that module is built.
    /// Keep the GUIDs stable during development so seeded/test data stays valid.
    /// </summary>
    public static class HardcodedMasterData
    {
        // TODO: replace with real tenant resolution (from logged-in user / auth claims)
        public static readonly Guid CurrentTenantId = new("11111111-1111-1111-1111-111111111111");

        public static readonly List<(Guid Id, string Name)> Branches = new()
        {
            (new Guid("22222222-2222-2222-2222-222222222221"), "Main Campus"),
            (new Guid("22222222-2222-2222-2222-222222222222"), "North Branch"),
            (new Guid("22222222-2222-2222-2222-222222222223"), "South Branch"),
        };

        public static readonly List<string> Genders = new() { "Male", "Female", "Other" };

        public static readonly List<string> StudentStatuses = new()
        {
            "Admitted", "Active", "Inactive", "Transferred", "Alumni", "Dropped"
        };

        public static List<SelectListItem> GetBranchSelectList(Guid? selected = null) =>
            Branches.ConvertAll(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name,
                Selected = selected.HasValue && selected.Value == b.Id
            });

        public static List<SelectListItem> GetGenderSelectList(string selected = null) =>
            Genders.ConvertAll(g => new SelectListItem
            {
                Value = g,
                Text = g,
                Selected = g == selected
            });

        public static List<SelectListItem> GetStatusSelectList(string selected = null) =>
            StudentStatuses.ConvertAll(s => new SelectListItem
            {
                Value = s,
                Text = s,
                Selected = s == selected
            });

        public static string GetBranchName(Guid branchId)
        {
            var branch = Branches.Find(b => b.Id == branchId);
            return branch.Name ?? "Unknown Branch";
        }
    }
}
