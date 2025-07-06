using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Utilities
{
	public static class StaticDetails
	{
		public const string RoleAdmin = "Admin";
		public const string RoleCustomer = "Customer";
		public const string RoleEmployee = "Employee";
		public const string RoleCompany = "Company";
		
		public const string OrderStatusPending = "Pending";
		public const string OrderStatusApproved = "Approved";
		public const string OrderStatusInProcess = "Processing";
		public const string OrderStatusShipped = "Shipped";
		public const string OrderStatusCancelled = "Cancelled";
		public const string OrderStatusRefunded = "Refunded";
		
		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected = "Rejected";
		
		public const string SessionCart = "SessionShoppingCart";
	}
}
