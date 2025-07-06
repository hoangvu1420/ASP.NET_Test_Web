namespace BookStore.DataAccess.DbInitializer;

public interface IDbInitializer
{
    // This method will be used to creating admin user and roles for the product database
    void Initialize();
}