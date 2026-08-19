using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Text;

namespace Resturant_Ordering_System.Domain.Abstract
{
    public interface IOrderRepo : IGenaricRepo<Orders,int>
    {
        public IQueryable<Orders> GetOrdersByCustomerId(string customerId);
        public IQueryable<Orders> SearchOrder(string searchKey);
        public IQueryable<Orders> GetOrdersWithDetails();
        public  Task<Orders> GetOrderWithDetails(int orderId);


    }
}
