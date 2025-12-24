using EZGO.Api.Models.General;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Middleware
{
    /// <summary>
    /// ExceptionMiddleware; Handling errors.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                if(ex.Message == AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN || ex.Message == AuthenticationSettings.MESSAGE_FAILED_APP_HAS_NO_ACCESS)
                {
                    _logger.LogWarning(message: $"Warning: {ex.Message}", exception: ex);
                } else if (ex.Message == AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS)
                {
                    _logger.LogWarning(message: $"Warning: {ex.Message}", exception: ex);
                } else
                {
                    _logger.LogError(message: $"Error occurred: {ex.Message}", exception: ex);
                }

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        /// <summary>
        /// HandleExceptionAsync; Handle exception that is occurring.
        /// </summary>
        /// <param name="context">Context where the response will be written to.</param>
        /// <param name="exception">Exception that is occurring.</param>
        /// <returns>Executing task.</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetCorrectStatusCode(exception: exception);

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = GetMessageBasedOnException(exception:exception)
            }.ToJsonFromObject());
        }

        /// <summary>
        /// GetCorrectStatusCode; Get a specific status code for certain exceptions.
        /// </summary>
        /// <param name="exception">Exception, type will be compared.</param>
        /// <returns>A status code (int)</returns>
        private int GetCorrectStatusCode(Exception exception)
        {
            if (exception is UnauthorizedAccessException)
            {
                return (int)HttpStatusCode.Unauthorized;
            }
            return (int)HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// GetMessageBasedOnException; Get a specific message or return the exception message.
        /// </summary>
        /// <param name="exception">Exception, type will be compared.</param>
        /// <returns>A message text for output usage.</returns>
        private string GetMessageBasedOnException(Exception exception)
        {
            if (exception is UnauthorizedAccessException)
            {
                return exception.Message;
            }
            return "A problem has occurred. Please try again later or contact the system administrator.";
        }
    }
}
