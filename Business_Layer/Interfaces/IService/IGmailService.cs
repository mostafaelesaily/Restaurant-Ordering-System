using Resturant_Ordering_System.Application.DTOs.EmailDTOs;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IGmailService
    {
        Task<string> GetAuthorizationUrlAsync();

        Task HandleOAuthCallbackAsync(string code);

        Task<SendEmailResponseDto> SendEmailAsync(SendEmailRequestDto request);
    }
}
