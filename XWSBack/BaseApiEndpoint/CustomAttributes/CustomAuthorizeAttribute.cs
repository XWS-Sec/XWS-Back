using System.Threading.Tasks;
using BaseApiModel.Graph;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;

namespace BaseApi.CustomAttributes
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly UserManager<User> _userManager;

        public CustomAuthorizeAttribute(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claims = context.HttpContext.User;
            var userId = _userManager.GetUserId(claims);
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            return base.OnActionExecutionAsync(context, next);
        }
    }
}