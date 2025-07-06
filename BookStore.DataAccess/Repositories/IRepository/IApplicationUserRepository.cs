using BookStore.Models;

namespace BookStore.DataAccess.Repositories.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        // No need to add any methods here because the Repository<T> class already has all the methods we need
    }
}
