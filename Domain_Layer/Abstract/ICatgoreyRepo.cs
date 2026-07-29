using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Abstract
{
    public interface ICatgoreyRepo : IGenaricRepo<Categories,int> 
    {
        IQueryable<Categories> Search_Catgorey_With_Name_Desc(string searchKey);
    }
}
