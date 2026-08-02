using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Domain.Abstract
{
    public interface ITableRepo : IGenaricRepo<Tables, int>
    {
        IQueryable<Tables> Search_Table_With_SearchKey(string searchKey);
        IQueryable<Tables> GetTablesByActiveStatus(bool status);
        IQueryable<Tables> GetTablesByCapacity(int capacity);
    }
}
