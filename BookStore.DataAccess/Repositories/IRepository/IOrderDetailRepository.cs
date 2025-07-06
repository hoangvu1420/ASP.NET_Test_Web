using BookStore.Models;

namespace BookStore.DataAccess.Repositories.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
