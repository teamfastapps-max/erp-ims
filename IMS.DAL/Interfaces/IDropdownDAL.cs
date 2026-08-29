using IMS.Models.Common.Dropdown;

namespace IMS.DAL.Interfaces
{
    /// <summary>
    /// Generic Dropdown Data Access Layer.
    /// Responsible for fetching dropdown data for all entities.
    /// </summary>
    public interface IDropdownDAL
    {
        /// <summary>
        /// Returns dropdown items based on the supplied configuration.
        /// Supports:
        /// - Normal dropdown
        /// - Cascading dropdown
        /// - Search
        /// - Active/Inactive filtering
        /// - Future pagination
        /// </summary>
        /// <param name="config">Dropdown configuration.</param>
        /// <param name="request">Dropdown request.</param>
        /// <returns>List of dropdown items.</returns>
        List<DropdownItemModel> GetDropdown(DropdownConfig config, DropdownRequestModel request);
    }
}
