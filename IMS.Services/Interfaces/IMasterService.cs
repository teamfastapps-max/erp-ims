// IMS.Services/Interfaces/IMasterService.cs
using System.Collections.Generic;

namespace IMS.Services.Interfaces
{
    public interface IMasterService
    {
        List<Dictionary<string, object>> GetAll(string entityType);
        Dictionary<string, object> GetById(string entityType, Guid id);
        (bool Success, string Message, Guid Id) Create(string entityType, Dictionary<string, object> values, Guid tenantId, Guid createdBy);
        (bool Success, string Message) Update(string entityType, Guid id, Dictionary<string, object> values, Guid tenantId, Guid updatedBy);
        (bool Success, string Message) Delete(string entityType, Guid id);
    }
}