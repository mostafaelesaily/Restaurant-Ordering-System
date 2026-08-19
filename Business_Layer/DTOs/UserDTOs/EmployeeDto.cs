using Resturant_Ordering_System.Domain.Enums;


namespace Resturant_Ordering_System.Application.DTOs.UserDTOs
{
    public class EmployeeDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public EmployeeRole Role { get; set; }
    }
}
