using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using Audacia.CodeAnalysis.Analyzers.Helpers.MethodLength;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore.Tests;

public class LogRequestBodyActionFilterAttributeTests
{
    private readonly Tuple<string, string> _returnUrl = new Tuple<string, string>("returnUrl", "someulr");

    private readonly Tuple<string, string> _requestId = new Tuple<string, string>("requestId", "0");

    [Test]
    [MaxMethodLength(12)]
    public async Task WhenUsingLogRequestBodyActionFilterItAddsActionArgumentsToContextItemsAsync()
    {
        IOptions<LogActionFilterConfig> config = Options.Create(new LogActionFilterConfig()
        {
            DisableBodyContent = false,
            ExcludeArguments = [],
            IncludeClaims = []
        });

        var filterAttribute = new LogRequestBodyActionFilterAttribute(config);

        var context = GetExecutingContextWithActionArgs(ClaimTypes.Sub, ClaimTypes.Role);

        ActionExecutionDelegate next = () =>
        {
            var actionExecutedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>());
            return Task.FromResult(actionExecutedContext);
        };

        Assert.That(context.HttpContext.Items, Has.Count.EqualTo(0));

        await filterAttribute.OnActionExecutionAsync(context, next);

        var actionArgs = (ActionArgumentDictionary)context.HttpContext.Items.Single(i => i.Key.ToString() == "ActionArguments").Value;
        
        Assert.That(actionArgs, Has.Count.EqualTo(2));

        Assert.That(actionArgs.Single(k => k.Key == _requestId.Item1).Value.ToString() == _requestId.Item2, Is.EqualTo(true));

        Assert.That(actionArgs.Single(k => k.Key == _returnUrl.Item1).Value.ToString() == _returnUrl.Item2, Is.EqualTo(true));
    }

    private ActionExecutingContext GetExecutingContextWithActionArgs(string userIdClaimType, string roleClaimType)
    {
        var modelState = new ModelStateDictionary();

        var httpContext = new DefaultHttpContext();
                
        httpContext.Request.Method = HttpMethod.Post.Method;

        var context = new ActionExecutingContext(
            new ActionContext(
                   httpContext: httpContext,
                   routeData: new RouteData(),
                   actionDescriptor: new ActionDescriptor() { FilterDescriptors = [] },
                   modelState: modelState),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>() { { _returnUrl.Item1, _returnUrl.Item2 }, { _requestId.Item1, _requestId.Item2 } },
            new Mock<Controller>().Object);

        return context;
    }
}
