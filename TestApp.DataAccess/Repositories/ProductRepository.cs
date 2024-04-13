using slnTestWeb.DataAccess.Data;
using slnTestWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TestApp.DataAccess.Repositories.IRepository;
using TestApp.Models;

namespace TestApp.DataAccess.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
	{
        // Since the CategoryRepository class implements the ICategoryRepository interface, and the ICategoryRepository interface inherits from the IRepository interface,
        // the CategoryRepository class must implement all the methods from the ICategoryRepository interface and the IRepository interface
        // For the methods from the IRepository interface, the input parameter will be replaced by a Category object instead of a generic object (T) like in the Repository class
        // To avoid the repetition of the methods from the IRepository interface, we can have the CategoryRepository class inherit from the Repository<Category> class

        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db) // We pass the ApplicationDbContext object to the base class (Repository<T>) constructor
        {
            _db = db;
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
