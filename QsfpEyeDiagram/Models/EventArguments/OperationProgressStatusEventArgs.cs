using System;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class OperationProgressStatusEventArgs : EventArgs
    {
        public bool IsOperationInProgress { get; private set; }

        public OperationProgressStatusEventArgs(bool isOperationInProgress)
        {
            IsOperationInProgress = isOperationInProgress;
        }
    }
}
