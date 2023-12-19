using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Headers;
using System.Text;

namespace TNEB.Shutdown.Notifier.Web.Utils
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

            if (allowAnonymous)
                return;

            BasicAuthUser? user = (BasicAuthUser?)context.HttpContext.Items["User"];

            if (user == null)
            {
                // not logged in - return 401 unauthorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };

                // set 'WWW-Authenticate' header to trigger login popup in browsers
                context.HttpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"\", charset=\"UTF-8\"";
            }
        }
    }

    public class BasicAuthUser
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public BasicAuthUser(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BasicAuthMiddleware> _logger;

        public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration)
        {
            try
            {
                string? header = context.Request.Headers["Authorization"];

                if (header == null)
                {
                    throw new Exception("Authorization header not found.");
                }

                string? encoded = AuthenticationHeaderValue.Parse(header).Parameter;

                if (encoded == null)
                {
                    throw new Exception("Authorization header not found.");
                }

                string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encoded)).Split(':', 2);

                var config = configuration.GetSection("BasicAuthUsers").GetChildren();

                if (config == null)
                {
                    throw new Exception("Basic auth users not configured.");
                }

                IEnumerable<BasicAuthUser> users = config.Select(i =>
                {
                    string? username = i.GetValue<string>("Username");
                    string? password = i.GetValue<string>("Password");

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        throw new Exception("Invalid basic auth user configuration.");
                    }

                    return new BasicAuthUser(username, password);
                });

                context.Items["User"] = users.First(u => u.Username.Equals(credentials[0], StringComparison.InvariantCultureIgnoreCase) && u.Password.Equals(credentials[1]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Basic auth failed!");
            }

            await _next(context);
        }
    }

    public static class BasicAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}
