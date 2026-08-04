using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using Resturant_Ordering_System.Domain.Abstract;

namespace Resturant_Ordering_System.Infrastructre.Repositories
{
    public class NotificationRepo : MainGenaricRepo<Notifications,int> , INotificationRepo
    {
        private readonly DbSet<Notifications> dbset;
        public NotificationRepo(AppDbContext context) : base(context)
        {
            dbset = context.Notifications;
        }
        public IQueryable<Notifications> GetUserNotifications(string userId)
        {
            return dbset.Where(n => n.UserId == userId);
        }
    }
}
