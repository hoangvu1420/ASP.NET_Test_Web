﻿using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repositories.IRepository;

namespace BookStore.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IApplicationUserRepository ApplicationUser { get; private set; }
    public ICategoryRepository Category { get; private set ;}
    public IProductRepository Product { get; private set; }
    public ICompanyRepository Company { get; private set; }
    public IShoppingCartRepository ShoppingCart { get; private set; }
    public IOrderHeaderRepository OrderHeader { get; private set; }
    public IOrderDetailRepository OrderDetail { get; private set; }
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        ApplicationUser = new ApplicationUserRepository(_db);
        Category = new CategoryRepository(_db);
        Product = new ProductRepository(_db);
        Company = new CompanyRepository(_db);
        ShoppingCart = new ShoppingCartRepository(_db);
        OrderHeader = new OrderHeaderRepository(_db);
        OrderDetail = new OrderDetailRepository(_db);
    }

    public void Save()
    {
        _db.SaveChanges();
    }
}