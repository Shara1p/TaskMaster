namespace TaskMaster.Middleware.Extensions;

public static class RequestLoggingMiddlewareExtension
{
    public static IApplicationBuilder UseRequestLogging(this WebApplication app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}