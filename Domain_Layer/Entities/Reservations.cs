using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Reservations
    {
        public int Id { get; set; }

        public string custoemerId { get; set; }

        public int tableId { get; set; }

        public DateTime ReservationDate { get; set; }

        public int NumberOfGuests { get; set; }

        public ReservationStatus Status { get; set; }

        public Tables Tables { get; set; }

        public AppUser User { get; set; }
    }
}
