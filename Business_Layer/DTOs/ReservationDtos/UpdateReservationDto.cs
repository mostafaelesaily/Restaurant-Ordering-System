using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReservationDtos
{
    public class UpdateReservationDto
    {
        public DateTime ReservationDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableId { get; set; }

    }
}
