using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class TeacherApiDirectoryLookup : ITeacherDirectoryLookup
    {
        private readonly ITeacherService _teacherService;

        public TeacherApiDirectoryLookup(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        public async Task<List<TeacherDirectoryEntry>> GetActiveTeachersAsync(Guid tenantId, string accessToken)
        {
            // pageSize: 500 stands in for "all active teachers, no paging" -
            // replace with a real unpaged roster call if/when one exists.
            var result = await _teacherService.GetTeacherListAsync(
                tenantId, accessToken, searchTerm: null, status: null, branchId: null, pageNumber: 1, pageSize: 500);

            return result.Teachers
                .Where(t => !t.IsPendingSetup) // no local Teachers_T row yet -> can't satisfy the FK, exclude from roster
                .Select(t => new TeacherDirectoryEntry
                {
                    Id = t.T_Id,
                    FullName = t.FullName
                })
                .ToList();
        }

        public async Task<string> GetTeacherNameAsync(Guid tenantId, Guid teacherId, string accessToken)
        {
            // Returns null for a pending teacher (profile == null in your
            // TeacherService) - callers must treat null as "can't proceed."
            var details = await _teacherService.GetTeacherDetailsAsync(teacherId, tenantId, accessToken);
            return details?.FullName;
        }
    }
}
