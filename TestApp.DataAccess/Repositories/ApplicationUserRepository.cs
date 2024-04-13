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
    public class ApplicationUserRepository(ApplicationDbContext db) : Repository<ApplicationUser>(db), IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db = db;
    }
}
