using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Notifications
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
        public AppUser User { get; set; }
    }
}
