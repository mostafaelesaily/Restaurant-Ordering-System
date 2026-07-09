using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.PaginatedDtos
{
    public class PaginatedResultDto<T>
    {
        public List<T> Data { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNext => PageNumber < TotalPages;

        public bool HasPrevious => PageNumber > 1;
    }
}
