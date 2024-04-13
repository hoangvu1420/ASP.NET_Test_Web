using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp.DataAccess.Repositories.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        // The ICategoryRepository interface inherits from the IRepository interface, so it has all the methods from the IRepository interface
        // However, we need to add the Update() and Save() methods to the ICategoryRepository interface so it will be specific to the Category model
        void Update(Product product);
    }
}
