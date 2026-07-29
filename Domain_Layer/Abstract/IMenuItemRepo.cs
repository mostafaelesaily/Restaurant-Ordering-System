using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business_Layer.Interfaces;

namespace Domain_Layer.Abstract
{
    public interface IMenuItemRepo : IGenaricRepo<MenuItems,int>
    {
        IQueryable<MenuItems> Search_MenuItem_With_Name_Desc(string searchKey);
        IQueryable<MenuItems> GetCategoreyMenuItems(int categoreyId);
    }
}
