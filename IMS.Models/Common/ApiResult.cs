using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Common
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }

        public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static ApiResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
    }
}
