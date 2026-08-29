using IMS.DAL.Interfaces;
using IMS.Models.Common.Dropdown;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    /// <summary>
    /// Generic Dropdown Service
    /// Responsible for validating requests and
    /// delegating dropdown retrieval to the DAL.
    /// </summary>
    public class DropdownService : IDropdownService
    {
        private readonly IDropdownDAL _dropdownDAL;

        public DropdownService(IDropdownDAL dropdownDAL)
        {
            _dropdownDAL = dropdownDAL;
        }

        /// <summary>
        /// Returns dropdown items for the specified entity.
        /// </summary>
        public List<DropdownItemModel> GetDropdown(DropdownRequestModel request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            var config = DropdownConfigRegistry.GetByEntityType(request.EntityType);

            if (config == null)
                throw new Exception(
                    $"Dropdown configuration not found for EntityType '{request.EntityType}'.");

            return _dropdownDAL.GetDropdown(config, request);
        }
    }
}