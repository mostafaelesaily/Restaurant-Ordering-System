using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.NotificationDTOs
{
    public class GetNotificationDto
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
