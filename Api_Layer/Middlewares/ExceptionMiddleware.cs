using Business_Layer.Exceptions;
using System.Text.Json;

namespace Api_Layer.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; 
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                switch (ex)
                {
                    case NotFoundException:
                        _logger.LogWarning(
                            ex,
                            "NotFoundException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        break;

                    case BadRequestException:
                        _logger.LogWarning(
                            ex,
                            "BadRequestException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        break;

                    case ConflictException:
                        _logger.LogWarning(
                            ex,
                            "ConflictException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        break;

                    case ForbiddenException:
                        _logger.LogWarning(
                            ex,
                            "ForbiddenException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        break;

                    case UnauthorizedException:
                        _logger.LogWarning(
                            ex,
                            "ForbiddenException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        break;

                    default:
                        _logger.LogError(
                            ex,
                            "Unhandled exception occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        break;
                }

                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response)
                );
            }
        }
    }
}
