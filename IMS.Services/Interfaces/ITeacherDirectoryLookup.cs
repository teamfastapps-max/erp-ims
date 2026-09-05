using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMS.Services.Interfaces
{
    public class TeacherDirectoryEntry
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
    }

    /// <summary>
    /// Isolates "get teacher name(s)" behind one small interface, since that
    /// data lives in the external Tenant User API (via your real Teacher
    /// module), not locally.
    ///
    /// Matches your real ITeacherService signatures: tenantId + accessToken
    /// are both required (your services never fetch tokens themselves - the
    /// token is always passed in from the controller's GetAccessTokenAsync()).
    /// </summary>
    public interface ITeacherDirectoryLookup
    {
        /// <summary>
        /// Active teachers with a COMPLETED local profile only (IsPendingSetup
        /// == false). Pending teachers are deliberately excluded here - they
        /// have no Teachers_T row yet, and both TeacherLeaves_TL and
        /// TeacherAttendance_TA have an FK to Teachers_T.T_Id, so including
        /// them in the attendance roster would only produce an FK violation
        /// the moment anyone tried to mark them.
        /// </summary>
        Task<List<TeacherDirectoryEntry>> GetActiveTeachersAsync(Guid tenantId, string accessToken);

        /// <summary>
        /// Returns null if this teacher has no completed local profile yet
        /// (pending setup) - callers MUST treat null as "can't proceed" and
        /// return a friendly error, not attempt the insert (same FK reason
        /// as above).
        /// </summary>
        Task<string> GetTeacherNameAsync(Guid tenantId, Guid teacherId, string accessToken);
    }
}
