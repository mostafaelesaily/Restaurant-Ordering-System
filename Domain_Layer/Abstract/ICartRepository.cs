using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Domain.Abstract
{
    public interface ICartRepository :  IGenaricRepo<Cart,int>
    {
        Task<Cart?> GetCartWithItemsAsync(string customerId);
        IQueryable<Cart> GetAllWithItems();

    }
}
