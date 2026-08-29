
using IMS.Models.Common.Dropdown;

namespace IMS.Services.Interfaces
{
    /// <summary>
    /// Generic Dropdown Service.
    /// Responsible for providing dropdown data for all modules.
    /// </summary>
    public interface IDropdownService
    {
        /// <summary>
        /// Returns dropdown items for the requested entity.
        /// Supports:
        /// - Normal dropdown
        /// - Cascading dropdown
        /// - Search
        /// - Active/Inactive filtering
        /// - Future pagination
        /// </summary>
        /// <param name="request">Dropdown request.</param>
        /// <returns>Dropdown items.</returns>
        List<DropdownItemModel> GetDropdown(DropdownRequestModel request);
    }
}