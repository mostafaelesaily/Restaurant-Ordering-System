using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReservationDtos
{
    public class ReservationDetailsDto
    {
        public int Id { get; set; }

        public DateTime ReservationDate { get; set; }

        public int NumberOfGuests { get; set; }

        public ReservationStatus Status { get; set; }

        public int TableId { get; set; }

        public string TableNumber { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }
    }
}
