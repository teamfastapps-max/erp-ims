// IMS.Services/MasterService.cs

using IMS.DAL.Interfaces;
using IMS.Models.Common.Master;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    /// <summary>
    /// Generic service layer for all master/lookup entities.
    /// Applies validation (required, unique, max length) driven by
    /// MasterConfig before delegating to MasterDAL.
    /// </summary>
    public class MasterService : IMasterService
    {
        private readonly IMasterDAL _masterDAL;

        public MasterService(IMasterDAL masterDAL)
        {
            _masterDAL = masterDAL;
        }

        public List<Dictionary<string, object>> GetAll(string entityType)
        {
            var config = GetConfigOrThrow(entityType);
            return _masterDAL.GetAll(config);
        }

        public Dictionary<string, object> GetById(string entityType, Guid id)
        {
            var config = GetConfigOrThrow(entityType);
            return _masterDAL.GetById(config, id);
        }

        public (bool Success, string Message, Guid Id) Create(string entityType, Dictionary<string, object> values, Guid tenantId, Guid createdBy)
        {
            var config = GetConfigOrThrow(entityType);

            var (isValid, errorMessage) = Validate(config, values, excludeId: null);
            if (!isValid)
                return (false, errorMessage, Guid.Empty);

            try
            {
                var newId = _masterDAL.Insert(config, values, tenantId, createdBy);
                return (true, $"{config.DisplayName} created successfully.", newId);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating {config.DisplayName}: {ex.Message}", Guid.Empty);
            }
        }

        public (bool Success, string Message) Update(string entityType, Guid id, Dictionary<string, object> values, Guid tenantId, Guid updatedBy)
        {
            var config = GetConfigOrThrow(entityType);

            var existing = _masterDAL.GetById(config, id);
            if (existing == null)
                return (false, $"{config.DisplayName} not found.");

            var (isValid, errorMessage) = Validate(config, values, excludeId: id);
            if (!isValid)
                return (false, errorMessage);

            try
            {
                var updated = _masterDAL.Update(config, id, values, tenantId, updatedBy);
                return updated
                    ? (true, $"{config.DisplayName} updated successfully.")
                    : (false, $"No changes were made to {config.DisplayName}.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating {config.DisplayName}: {ex.Message}");
            }
        }

        public (bool Success, string Message) Delete(string entityType, Guid id)
        {
            var config = GetConfigOrThrow(entityType);

            var existing = _masterDAL.GetById(config, id);
            if (existing == null)
                return (false, $"{config.DisplayName} not found.");

            try
            {
                var deleted = _masterDAL.Delete(config, id);
                var action = config.SoftDelete ? "deactivated" : "deleted";
                return deleted
                    ? (true, $"{config.DisplayName} {action} successfully.")
                    : (false, $"Failed to delete {config.DisplayName}.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting {config.DisplayName}: {ex.Message}");
            }
        }

        // ---------- Private helpers ----------

        private MasterConfig GetConfigOrThrow(string entityType)
        {
            var config = MasterConfigRegistry.GetByEntityType(entityType);
            if (config == null)
                throw new ArgumentException($"No master configuration found for entity type '{entityType}'.");
            return config;
        }

        private (bool IsValid, string ErrorMessage) Validate(MasterConfig config, Dictionary<string, object> values, Guid? excludeId)
        {
            foreach (var field in config.Fields)
            {
                values.TryGetValue(field.ColumnName, out var rawValue);
                var stringValue = rawValue?.ToString();

                // Required check
                if (field.IsRequired && string.IsNullOrWhiteSpace(stringValue))
                {
                    return (false, $"{field.DisplayName} is required.");
                }

                // Max length check
                if (field.MaxLength.HasValue && !string.IsNullOrEmpty(stringValue)
                    && stringValue.Length > field.MaxLength.Value)
                {
                    return (false, $"{field.DisplayName} must not exceed {field.MaxLength.Value} characters.");
                }

                // Date-specific validation
                if (field.FieldType == MasterFieldType.Date && !string.IsNullOrWhiteSpace(stringValue))
                {
                    if (!DateTime.TryParse(stringValue,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedDate))
                    {
                        return (false, $"Please enter a valid date for {field.DisplayName}.");
                    }

                    // Min date check
                    if (field.MinDate.HasValue && parsedDate < field.MinDate.Value)
                    {
                        return (false, $"{field.DisplayName} cannot be before {field.MinDate.Value:dd/MM/yyyy}.");
                    }

                    // Max date check
                    if (field.MaxDate.HasValue && parsedDate > field.MaxDate.Value)
                    {
                        return (false, $"{field.DisplayName} cannot be after {field.MaxDate.Value:dd/MM/yyyy}.");
                    }

                    // Future date check
                    if (!field.AllowFutureDates && parsedDate > DateTime.Today)
                    {
                        return (false, $"{field.DisplayName} cannot be a future date.");
                    }

                    // Past date check
                    if (!field.AllowPastDates && parsedDate < DateTime.Today)
                    {
                        return (false, $"{field.DisplayName} cannot be a past date.");
                    }
                }

                // Uniqueness check
                if (field.IsUnique && !string.IsNullOrWhiteSpace(stringValue))
                {
                    var exists = _masterDAL.ExistsByField(config, field.ColumnName, stringValue, excludeId);
                    if (exists)
                    {
                        return (false, $"{field.DisplayName} '{stringValue}' already exists.");
                    }
                }
            }

            // Date range validation (e.g., End Date must be after Start Date)
            foreach (var field in config.Fields)
            {
                if (field.FieldType == MasterFieldType.Date
                    && !string.IsNullOrEmpty(field.DateRangeStartField)
                    && !string.IsNullOrEmpty(field.DateRangeEndField))
                {
                    values.TryGetValue(field.DateRangeStartField, out var startRaw);
                    values.TryGetValue(field.DateRangeEndField, out var endRaw);

                    var startStr = startRaw?.ToString();
                    var endStr = endRaw?.ToString();

                    if (!string.IsNullOrWhiteSpace(startStr) && !string.IsNullOrWhiteSpace(endStr))
                    {
                        if (DateTime.TryParse(startStr, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var startDate)
                            && DateTime.TryParse(endStr, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var endDate))
                        {
                            if (endDate < startDate)
                            {
                                return (false, "End Date cannot be before Start Date.");
                            }
                        }
                    }
                }
            }

            return (true, null);
        }
    }
}