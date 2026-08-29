// IMS.DAL/Interfaces/IMasterDAL.cs
using IMS.Models.Common.Master;

namespace IMS.DAL.Interfaces
{
    public interface IMasterDAL
    {
        List<Dictionary<string, object>> GetAll(MasterConfig config);
        Dictionary<string, object> GetById(MasterConfig config, Guid id);
        Guid Insert(MasterConfig config, Dictionary<string, object> values, Guid tenantId, Guid createdBy);
        bool Update(MasterConfig config, Guid id, Dictionary<string, object> values, Guid tenantId, Guid updatedBy);
        bool Delete(MasterConfig config, Guid id); // soft or hard, based on config.SoftDelete
        bool ExistsByField(MasterConfig config, string columnName, object value, Guid? excludeId = null);
    }
}