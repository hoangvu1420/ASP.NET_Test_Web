using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;

namespace BookStore.DataAccess.Repositories
{
    public class ApplicationUserRepository(ApplicationDbContext db) : Repository<ApplicationUser>(db), IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db = db;
    }
}
