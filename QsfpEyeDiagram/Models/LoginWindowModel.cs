using QsfpEyeDiagram.Database;
using Std.Data.Database.Domain;
using System;
using System.Globalization;

using System.Windows;

namespace QsfpEyeDiagram.Models
{
    public class LoginWindowModel
    {
        private static readonly WorkerRecord _unknownWorker = new WorkerRecord()
        {
            Code = "unknown",
            FullName = null,
            Login = "Неавторизованный пользователь",
            Password = null,
            Description = null
        };

        public LoginWindowModel()
        {
        }

        public OperationResult LoginToApplication(string login, string password)
        {
            WorkerRecord worker;

            try
            {
                
                var repositoty = new QsfpEyeDiagramRepository<WorkerRecord, int>(QsfpEyeDiagramDataContext.Instance);
                var service = new WorkerService(repositoty);

                worker = service.GetWorkerByLoginAndPassword(login, password);
            }
            catch
            {
                // Попытка входа в приложение путем проверки соответствия 
                var dateTime = DateTime.Now;
                if (IsValidOfflineLogin(login, dateTime) && IsValidOfflinePassword(password, dateTime))
                {
                    OpenMainWindow(_unknownWorker);
                    CloseLoginWindow();

                    return new OperationResult(OperationStatuses.Success, "Выполнен вход под неавторизованным пользователем.");
                }

                return new OperationResult(OperationStatuses.Failure, "Не удалось подключиться к базе данных.");
            }

            if (worker != null)
            {
                OpenMainWindow(worker);
                CloseLoginWindow();

                return new QsfpEyeDiagram.OperationResult(OperationStatuses.Success, "Авторизация пройдена успешно.");
            }
            else
            {
                return new QsfpEyeDiagram.OperationResult(OperationStatuses.Failure, "Введены неверные логин и пароль.");
            }
        }

        private void OpenMainWindow(WorkerRecord worker)
        {
            var mainWindow = new MainWindow(worker);
            mainWindow.Show();
        }

        internal void CloseLoginWindow()
        {
            foreach (var element in Application.Current.Windows)
            {
                var window = element as Window;
                if (window.GetType().Name == typeof(LoginWindow).Name)
                {
                    window.Close();
                    break;
                }
            }
        }

        private bool IsValidOfflineLogin(string login, DateTime dateTime)
        {
            return login == $"{dateTime.Hour:D2}{dateTime.Day:D2}";
        }

        private bool IsValidOfflinePassword(string password, DateTime dateTime)
        {
            return password == $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(dateTime.Month)}{dateTime.Year:D4}";
        }
    }
}
