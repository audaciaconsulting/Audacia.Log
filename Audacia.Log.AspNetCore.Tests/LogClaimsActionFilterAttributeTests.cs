using System.Security.Claims;
using Audacia.CodeAnalysis.Analyzers.Helpers.MethodLength;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;

namespace Audacia.Log.AspNetCore.Tests;

public class LogClaimsActionFilterAttributeTests
{
    private readonly string _userId = Guid.NewGuid().ToString();

    private readonly string _role = "admin";

    [Test]
    public async Task WhenUsingLogClaimsActionFilterItAddsUserIdAndRoleToHttpContextItemsAsync()
    {
        IOptions<LogActionFilterConfig> config = Options.Create(new LogActionFilterConfig());

        var filterAttribute = new LogClaimsActionFilterAttribute(config);

        var context = GetExecutingContext(ClaimTypes.Sub, ClaimTypes.Role);

        ActionExecutionDelegate next = () =>
        {
            var actionExecutedContext = new ActionExecutedContext(context, [], Mock.Of<Controller>());
            return Task.FromResult(actionExecutedContext);
        };

        Assert.That(context.HttpContext.Items, Has.Count.EqualTo(0));

        await filterAttribute.OnActionExecutionAsync(context, next);

        Assert.That(context.HttpContext.Items, Has.Count.EqualTo(2));

        Assert.That(context.HttpContext.Items.Single(i => i.Key.ToString() == "ActionUserId").Value.ToString() == _userId, Is.EqualTo(true));

        Assert.That(context.HttpContext.Items.Single(i => i.Key.ToString() == "ActionUserRoles").Value.ToString() == _role, Is.EqualTo(true));
    }

    [Test]
    public async Task WhenUsingLogClaimsActionFilterYouCanOverrideUserIdAndRoleTypeAsync()
    {
        IOptions<LogActionFilterConfig> config = Options.Create(new LogActionFilterConfig()
        {
            IdClaimType = "oid",
            RoleClaimType = "access"
        });

        var filterAttribute = new LogClaimsActionFilterAttribute(config);

        var context = GetExecutingContext("oid", "access");

        ActionExecutionDelegate next = () =>
        {
            var actionExecutedContext = new ActionExecutedContext(context, [], Mock.Of<Controller>());
            return Task.FromResult(actionExecutedContext);
        };

        await filterAttribute.OnActionExecutionAsync(context, next);

        Assert.That(context.HttpContext.Items.Single(i => i.Key.ToString() == "ActionUserId").Value.ToString() == _userId, Is.EqualTo(true));

        Assert.That(context.HttpContext.Items.Single(i => i.Key.ToString() == "ActionUserRoles").Value.ToString() == _role, Is.EqualTo(true));
    }

    private ActionExecutingContext GetExecutingContext(string userIdClaimType, string roleClaimType)
    {
        var modelState = new ModelStateDictionary();

        var claims = new List<Claim>()
        {
                new(userIdClaimType, _userId),
                new(roleClaimType, _role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");

        var contextUser = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext()
        {
            User = contextUser
        };

        var context = new ActionExecutingContext(
            new ActionContext(
                   httpContext: httpContext,
                   routeData: new RouteData(),
                   actionDescriptor: new ActionDescriptor() { FilterDescriptors = [] },
                   modelState: modelState),
            [],
            new Dictionary<string, object>(),
            new Mock<Controller>().Object);

        return context;
    }
}