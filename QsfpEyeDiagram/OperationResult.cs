using System;
using System.Text;

namespace QsfpEyeDiagram
{
    public class OperationResult
    {
        public static readonly OperationResult Success = new OperationResult(OperationStatuses.Success);
        public static readonly OperationResult PartialSuccess = new OperationResult(OperationStatuses.PartialSuccess);
        public static readonly OperationResult Failure = new OperationResult(OperationStatuses.Failure);

        public OperationStatuses Status { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public OperationResult InnerResult { get; private set; }

        public OperationResult(OperationStatuses status)
        {
            Status = status;
        }

        public OperationResult(OperationStatuses status, string message) : this(status)
        {
            Message = message;
        }

        public OperationResult(OperationStatuses status, OperationResult innerResult) : this(status)
        {
            InnerResult = innerResult;
        }

        public OperationResult(OperationStatuses status, string message, OperationResult innerResult) : this(status, message)
        {
            InnerResult = innerResult;
        }

        public string GetAllMessages()
        {
            if (InnerResult == null)
            {
                return Message;
            }

            // Формирование строки из всех сообщений, отделенных символом переноса строки.
            var stringBuilder = new StringBuilder(Message + Environment.NewLine, 64);
            var innerResult = InnerResult;

            // Рекурсивный перебор всех внутренних результатов.
            while (innerResult != null)
            {
                stringBuilder.Append(innerResult.Message + Environment.NewLine);
                innerResult = innerResult.InnerResult;
            }

            return stringBuilder.ToString();
        }
    }
}
