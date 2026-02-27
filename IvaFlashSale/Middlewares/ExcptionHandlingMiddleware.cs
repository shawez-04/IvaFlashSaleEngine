using IvaFlashSaleEngine.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IvaFlashSaleEngine.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";
            var errorCode = "INTERNAL_SERVER_ERROR";

            if (exception is ServiceException serviceEx)
            {
                statusCode = serviceEx.StatusCode;
                message = serviceEx.Message;
                errorCode = serviceEx.ErrorCode;
                _logger.LogWarning("Service logic error: {ErrorCode} - {Message}", errorCode, message);
            }
            else if (exception is DbUpdateConcurrencyException)
            {
                statusCode = HttpStatusCode.Conflict;
                message = "High traffic detected. Please try your purchase again.";
                errorCode = "CONCURRENCY_CONFLICT";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = message,
                errorCode = errorCode,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}