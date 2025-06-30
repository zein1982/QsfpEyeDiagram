using Std.Data.Database.Base;
using Std.Data.Database.Domain;
using Std.Data.Database.Interfaces;
using System.Linq;

namespace QsfpEyeDiagram.Database
{
    public class WorkerService : BaseService<WorkerRecord>
    {
        public WorkerService(IRepository<WorkerRecord, int> repository) : base(repository)
        {
        }

        public WorkerRecord GetWorkerByLoginAndPassword(string login, string password)
        {
            var worker = repository.Where(g => g.Login == login && g.Password == password && g.Active!=0).FirstOrDefault();
            return worker;
        }
    }
}
