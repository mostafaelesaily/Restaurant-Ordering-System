using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReservationDtos
{
    public class CreateReservationDto
    {
        public int TableId { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration { get; set; } 
        public DateTime EndTime { get; set; } = DateTime.UtcNow ;
        public int NumberOfGuests { get; set; }

    }
}
