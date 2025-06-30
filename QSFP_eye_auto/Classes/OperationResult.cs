using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSFP_eye_auto.Classes
{
    public class OperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public OperationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
