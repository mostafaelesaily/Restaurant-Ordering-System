using Business_Layer.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.AiDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resturant_Ordering_System.Infrastructre.Services
{
    public class AiService : IAiService
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly ILogger<AiService> logger;

        public AiService(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<AiService> logger)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<AIResponseDto> GenerateResponseAsync(AIRequestDto request)
        {
            var apiKey = configuration["Groq:ApiKey"];
            var model = configuration["Groq:Model"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogCritical("Groq API key is not configured.");
                throw new InvalidOperationException("Groq API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                logger.LogCritical("Groq model is not configured.");
                throw new InvalidOperationException("Groq model is not configured.");
            }

            var url = "https://api.groq.com/openai/v1/chat/completions";

            var body = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = request.Request
                    }
                }
            };

            var content = JsonContent.Create(body);

            try
            {
                logger.LogInformation("Sending request to Groq API.");

                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                httpRequest.Content = content;

                var response = await httpClient.SendAsync(httpRequest);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError(
                        "Groq API Error. StatusCode: {StatusCode}, Response: {Response}",
                        response.StatusCode,
                        responseBody);

                    throw new HttpRequestException(
                        $"Groq API returned {(int)response.StatusCode}: {responseBody}");
                }


                using var document =
                    JsonDocument.Parse(responseBody);

                var generatedContent = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return new AIResponseDto
                {
                    Content = generatedContent ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred while sending request to Groq API.");

                throw;
            }
        }
    }
}