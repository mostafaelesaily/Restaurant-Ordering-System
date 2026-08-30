using Resturant_Ordering_System.Application.DTOs.AiDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IAiService
    {
        Task<AIResponseDto> GenerateResponseAsync(AIRequestDto prompt);
    }
}
