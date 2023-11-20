namespace ItemsApi;

/// <summary>
/// Sets a custom response header with the name of the originating host.
/// Mostly used to verify load balancing is working
/// </summary>
public class LBInfoFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
       var result = await next(context);
       context.HttpContext.Response.Headers.Append("X-LB-HOST", context.HttpContext.Request.Host.Value);
       return result;
    }
}
