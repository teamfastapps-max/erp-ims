using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Helpers.Constants
{
    public class AppPermissionDefinition
    {
        public string Id { get; set; }
        public string FeatureKey { get; set; }
        public string FeatureDisplayName { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }
    }

    public static class Permissions
    {
        public const string AddVendor = "ADD_VENDOR";
        public const string DeleteVendor = "DELETE_VENDOR";
        public const string UpdateVendor = "Update_VENDOR";
        public const string ReadVendor = "Read_VENDOR";


        private const string ServiceName = "erp-IMS-service";
        private const string VendorFeature = "VENDOR_MANAGEMENT";
        private const string VendorFeatureDisplay = "Vendor Management";

        public static readonly List<AppPermissionDefinition> All = new()
        {
            new() { Id = AddVendor, FeatureKey = VendorFeature, FeatureDisplayName = VendorFeatureDisplay, Description = "Add new vendor", ServiceName = ServiceName },
            new() { Id = DeleteVendor, FeatureKey = VendorFeature, FeatureDisplayName = VendorFeatureDisplay, Description = "Delete vendor", ServiceName = ServiceName },
            new() { Id = UpdateVendor, FeatureKey = VendorFeature, FeatureDisplayName = VendorFeatureDisplay, Description = "Update VENDOR", ServiceName = ServiceName },
            new() { Id = ReadVendor, FeatureKey = VendorFeature, FeatureDisplayName = VendorFeatureDisplay, Description = "Read VENDOR", ServiceName = ServiceName },

        };
    }
}
