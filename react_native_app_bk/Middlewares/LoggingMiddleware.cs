using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the request
        await LogRequest(context);

        // Enable buffering for the response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Continue down the middleware pipeline
        await _next(context);

        // Log the response
        await LogResponse(context, responseBody, originalBodyStream);
    }

    private async Task LogRequest(HttpContext context)
    {
        var requestBody = string.Empty;
        context.Request.EnableBuffering();

        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var logMessage = $@"
===================================================================================
HTTP Request Information
--------------------------
Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}
Method: {context.Request.Method}
Path: {context.Request.GetDisplayUrl()}
Client IP: {context.Connection.RemoteIpAddress}
Content Length: {context.Request.ContentLength}
Content Type: {context.Request.ContentType}
Request Body: {requestBody}
===================================================================================";

        _logger.LogInformation(logMessage);
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        var logMessage = $@"
===================================================================================
HTTP Response Information
--------------------------
Status Code: {context.Response.StatusCode}
Content Type: {context.Response.ContentType}
Response Body: {responseContent}
===================================================================================";

        _logger.LogInformation(logMessage);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}

// Extension method to make it easier to add the middleware
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}