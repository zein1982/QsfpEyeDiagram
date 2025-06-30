using Microsoft.EntityFrameworkCore;
using Std.Data.Database.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace QsfpEyeDiagram.Database
{
    public class QsfpEyeDiagramRepository<T, TPrimaryKey> : IRepository<T, TPrimaryKey> where T : class, IEntity<TPrimaryKey>
    {
        private readonly QsfpEyeDiagramDataContextInstance _context;
        public DbSet<T> Table { get; private set; }

        public QsfpEyeDiagramRepository(QsfpEyeDiagramDataContextInstance context)
        {
            _context = context;
            Table = context.Set<T>();
        }

        public TPrimaryKey Add(T entity)
        {
            Table.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }

        public IQueryable<T> All()
        {
            return Table;
        }

        public void Delete(T entity)
        {
            Table.Remove(entity);
            _context.SaveChanges();
        }

        public T GetById(TPrimaryKey id)
        {
            return Table.FirstOrDefault(c => c.Id.Equals(id));
        }

        public void Update(T entity)
        {
            Table.Update(entity);
            _context.SaveChanges();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> where)
        {
            return Table.Where(where);
        }
    }
}
