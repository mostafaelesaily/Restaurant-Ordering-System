using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Tables
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }

        public int Capacity { get; set; }

        public string? QrCode { get; set; }

        public bool isActive { get; set; }

        public ICollection<Orders> orders { get; set; } = new List<Orders>();
        public ICollection<Reservations> reservations { get; set; } = new List<Reservations>();
    }
}
