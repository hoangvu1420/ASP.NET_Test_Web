using System.Linq.Expressions;
using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;

        protected Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>();
            _db.Products.Include(p =>
                p.Category); // This code will include the Category property of the Product class when we get a product from the database.
        }

        public void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public T Get(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool isTracking = false)
        {
            // The isTracking parameter is used to specify whether the entity should be tracked by the context or not.
            // The default value of this parameter is false, which means that the entity will not be tracked by the context.
            IQueryable<T> query = _dbSet;
            if (isTracking == false)
            {
                query = query.AsNoTracking();
            }

            query = query.Where(predicate);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null) {
                query = query.Where(filter);
            }
			if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public IEnumerable<T> GetRange(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool isTracking = false)
        {
            IQueryable<T> query = _dbSet;
            if (isTracking == false)
            {
                query = query.AsNoTracking();
            }

            query = query.Where(predicate);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query.ToList();
        }
    }
}
