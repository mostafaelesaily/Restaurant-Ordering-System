using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.TablesDTOs
{
    public class GetTablesDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string? QrCode { get; set; }
        public bool isActive { get; set; }
    }
}
