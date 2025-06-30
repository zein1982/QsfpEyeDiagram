using System;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class OperationSuccessEventArgs : EventArgs
    {
        public bool Success { get; private set; }

        public OperationSuccessEventArgs(bool success)
        {
            Success = success;
        }
    }
}
