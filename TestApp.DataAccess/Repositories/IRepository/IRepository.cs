using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.DataAccess.Repositories.IRepository
{
    public interface IRepository<T> where T : class
    {
		IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        // The GetAll() method returns all entities of type T. The method accepts two optional parameters: filter and includeProperties.
        // The filter parameter is a lambda expression that takes in a T and returns a bool. The filter parameter is used to filter the entities.
        // The includeProperties parameter is a comma-separated list of navigation properties to include in the query.

		T Get(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool isTracking = false); // This is a delegate that represents a lambda expression that takes in a T and returns a bool
        // For example, if you have a class called Person, you can use this method to get a person by their name with the following code:
        // Person person = Get(p => p.Name == "John"); in this case, p is the parameter and p.Name == "John" is the expression
        IEnumerable<T> GetRange(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool isTracking = false);
        void Insert(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
