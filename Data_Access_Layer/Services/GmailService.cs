using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Resturant_Ordering_System.Application.DTOs.EmailDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Infrastructre.Services
{
    public class GmailService : IGmailService
    {
        private const string UserKey = "gmail-user";
        private const string TokenStoreFolder = "GoogleTokens";
        private const string ApplicationName = "Restaurant Ordering System";

        private readonly IConfiguration configuration;
        private readonly ILogger<GmailService> logger;

        public GmailService(
            IConfiguration configuration,
            ILogger<GmailService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task<string> GetAuthorizationUrlAsync()
        {
            try
            {
                var (_, _, redirectUri) = GetValidatedConfiguration();
                var flow = CreateAuthorizationFlow();

                var authorizationUrl = flow
                    .CreateAuthorizationCodeRequest(redirectUri)
                    .Build()
                    .ToString();

                logger.LogInformation(
                    "Gmail OAuth authorization URL generated successfully.");

                return Task.FromResult(authorizationUrl);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                logger.LogError(ex, "Failed to generate Gmail OAuth authorization URL.");
                throw;
            }
        }

        public async Task HandleOAuthCallbackAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(
                    "Authorization code is required.",
                    nameof(code));
            }

            try
            {
                var (_, _, redirectUri) = GetValidatedConfiguration();
                var flow = CreateAuthorizationFlow();

                await flow.ExchangeCodeForTokenAsync(
                    UserKey,
                    code,
                    redirectUri,
                    CancellationToken.None);

                logger.LogInformation(
                    "Gmail OAuth authorization completed successfully.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException
                and not ArgumentException)
            {
                logger.LogError(ex, "Failed to complete Gmail OAuth authorization.");
                throw;
            }
        }

        public async Task<SendEmailResponseDto> SendEmailAsync(
            SendEmailRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.To))
            {
                throw new ArgumentException(
                    "Recipient email address is required.",
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new ArgumentException(
                    "Email subject is required.",
                    nameof(request));
            }

            try
            {
                var flow = CreateAuthorizationFlow();

                var token = await flow.LoadTokenAsync(
                    UserKey,
                    CancellationToken.None);

                if (token == null)
                {
                    logger.LogWarning(
                        "Gmail account has not been authorized yet.");

                    return new SendEmailResponseDto
                    {
                        Success = false,
                        Message = "Gmail account has not been authorized yet."
                    };
                }

                var credential = new UserCredential(flow, UserKey, token);

                var gmailApiService = new Google.Apis.Gmail.v1.GmailService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName
                    });

                var mimeMessage = BuildMimeMessage(
                    request.To,
                    request.Subject,
                    request.Body);

                var rawMessage = EncodeMimeMessage(mimeMessage);

                var gmailMessage = new Message
                {
                    Raw = rawMessage
                };

                await gmailApiService.Users.Messages
                    .Send(gmailMessage, "me")
                    .ExecuteAsync();

                logger.LogInformation(
                    "Email sent successfully to {Recipient}.",
                    request.To);

                return new SendEmailResponseDto
                {
                    Success = true,
                    Message = "Email sent successfully."
                };
            }
            catch (Exception ex) when (ex is not InvalidOperationException
                and not ArgumentException
                and not ArgumentNullException)
            {
                logger.LogError(ex, "Failed to send email via Gmail API.");

                return new SendEmailResponseDto
                {
                    Success = false,
                    Message = "Failed to send email via Gmail API."
                };
            }
        }

        private GoogleAuthorizationCodeFlow CreateAuthorizationFlow()
        {
            var (clientId, clientSecret, _) = GetValidatedConfiguration();

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            return new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = new[]
                    {
                        Google.Apis.Gmail.v1.GmailService.Scope.GmailSend
                    },
                    DataStore = new FileDataStore(TokenStoreFolder)
                });
        }

        private (string ClientId, string ClientSecret, string RedirectUri)
            GetValidatedConfiguration()
        {
            var clientId = configuration["Gmail:ClientId"];
            var clientSecret = configuration["Gmail:ClientSecret"];
            var redirectUri = configuration["Gmail:RedirectUri"];

            if (string.IsNullOrEmpty(clientId))
            {
                logger.LogCritical(
                    "Google Gmail ClientId is missing from configuration.");

                throw new InvalidOperationException(
                    "Google Gmail ClientId is missing.");
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                logger.LogCritical(
                    "Google Gmail ClientSecret is missing from configuration.");

                throw new InvalidOperationException(
                    "Google Gmail ClientSecret is missing.");
            }

            if (string.IsNullOrEmpty(redirectUri))
            {
                logger.LogCritical(
                    "Google Gmail RedirectUri is missing from configuration.");

                throw new InvalidOperationException(
                    "Google Gmail RedirectUri is missing.");
            }

            return (clientId, clientSecret, redirectUri);
        }

        private static MimeMessage BuildMimeMessage(
            string to,
            string subject,
            string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                "Resturant Ordering System"
               ,"elesialy@gmail.com"));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body ?? string.Empty
            };

            return message;
        }

        private static string EncodeMimeMessage(MimeMessage message)
        {
            using var stream = new MemoryStream();
            message.WriteTo(stream);

            return Convert.ToBase64String(stream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
