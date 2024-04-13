using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.DataAccess.DbInitializer;

public interface IDbInitializer
{
    // This method will be used to creating admin user and roles for the product database
    void Initialize();
}