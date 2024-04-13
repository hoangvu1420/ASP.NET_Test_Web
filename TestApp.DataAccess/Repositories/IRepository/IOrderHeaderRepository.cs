using TestApp.Models;

namespace TestApp.DataAccess.Repositories.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int orderHeaderId, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentId(int orderHeaderId, string sessionId, string paymentIntentId);
    }
}
