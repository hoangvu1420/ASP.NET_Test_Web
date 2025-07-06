using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;

namespace BookStore.DataAccess.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        // Since the CategoryRepository class implements the ICategoryRepository interface, and the ICategoryRepository interface inherits from the IRepository interface,
        // the CategoryRepository class must implement all the methods from the ICategoryRepository interface and the IRepository interface
        // For the methods from the IRepository interface, the input parameter will be replaced by a Category object instead of a generic object (T) like in the Repository class
        // To avoid the repetition of the methods from the IRepository interface, we can have the CategoryRepository class inherit from the Repository<Category> class

        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db) // We pass the ApplicationDbContext object to the base class (Repository<T>) constructor
        {
            _db = db;
        }

        public void Update(Company company)
        {
            _db.Companies.Update(company);
        }
    }
}
