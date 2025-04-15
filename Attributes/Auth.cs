using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using whook.services;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdmin : Attribute, IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    if (!context.HttpContext.Request.Headers.TryGetValue("X-ADMIN-KEY", out var extractedApiKey))
    {
      context.Result = new ContentResult()
      {
        StatusCode = 401,
                   Content = "Missing Key"
      };
      return;
    }
    string admin_key = extractedApiKey.ToString(); 

    IScriptService service = context.HttpContext.RequestServices.GetRequiredService<IScriptService>();
    if (!service.IsAdmin(admin_key))
    {
      context.Result = new ContentResult()
      {
        StatusCode = 403,
                   Content = "Unauthorized Key"
      };
      return;
    }

    await next();
  }
}
