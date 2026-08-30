using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IExpenseCategoryDAL
    {
        Task<List<ExpenseCategory>> GetAllAsync(Guid tenantId);
    }
}
