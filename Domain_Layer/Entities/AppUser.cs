using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class AppUser : IdentityUser
    {
        public Cart cart {  get; set; }

        public ICollection<Orders> Userorders { get; set; } = new List<Orders>();
        public ICollection<Orders> Cheiforders { get; set; } = new List<Orders>();
        public ICollection<Orders> Deliveryorders { get; set; } = new List<Orders>();
        public ICollection<RefreshTokens> RefreshTokens { get; set; } = new List<RefreshTokens>();
        public ICollection<Notifications> Notifications { get; set; } = new List<Notifications>();
        public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
    }
}
