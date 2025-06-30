using System;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class OperationTypeEventArgs : EventArgs
    {
        public bool IsIndefinitelyLongOperation { get; private set; }

        public OperationTypeEventArgs(bool isIndefinitelyLongOperation)
        {
            IsIndefinitelyLongOperation = isIndefinitelyLongOperation;
        }
    }
}
