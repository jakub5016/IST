using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeService.API.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]

    public class RoleCheck : Attribute, IActionFilter
    {
        private List<string> _allowedRoles = [Roles.Admin];
        private const string RoleHeader = "x-jwt-role";

        public RoleCheck(params string[] allowedRoles)
        {
            _allowedRoles.AddRange([.. allowedRoles]);

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;

            if (!headers.TryGetValue(RoleHeader, out var actualRole) || !_allowedRoles.Contains(actualRole.FirstOrDefault()))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = $"Access denied."
                };
            }
        }
    }
}
