using BookStore.Models;

namespace BookStore.DataAccess.Repositories.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        // The ICategoryRepository interface inherits from the IRepository interface, so it has all the methods from the IRepository interface
        // However, we need to add the Update() and Save() methods to the ICategoryRepository interface so it will be specific to the Category model
        void Update(Product product);
    }
}
