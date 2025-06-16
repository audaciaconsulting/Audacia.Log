using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using Audacia.Log.AspNetCore.Configuration;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore.Tests;

public class LogRequestBodyActionFilterAttributeTests
{
    private readonly Tuple<string, string> _returnUrl = new("returnUrl", "someUrl");

    private readonly Tuple<string, string> _requestId = new("requestId", "0");

    [Test]
    public async Task WhenUsingLogRequestBodyActionFilterItAddsActionArgumentsToContextItemsAsync()
    {
        IOptions<LogActionFilterConfig> config = Options.Create(
            new LogActionFilterConfig()
            {
                DisableBodyContent = false,
                ExcludeArguments = [],
                IncludeClaims = []
            });

        var filterAttribute = new LogRequestBodyActionFilterAttribute(config);

        var context = GetExecutingContextWithActionArgs();

        Assert.That(context.HttpContext.Items, Has.Count.EqualTo(0));

        await filterAttribute.OnActionExecutionAsync(context, Next);

        var actionArgs = (RedactionDictionary)context.HttpContext.Items
            .Single(keyValuePair => keyValuePair.Key.ToString() == "ActionArguments").Value;

        Assert.That(actionArgs, Has.Count.EqualTo(2));
        Assert.That(
            actionArgs.Single(keyValuePair => keyValuePair.Key == _requestId.Item1).Value.ToString() ==
            _requestId.Item2,
            Is.EqualTo(true));
        Assert.That(
            actionArgs.Single(keyValuePair => keyValuePair.Key == _returnUrl.Item1).Value.ToString() ==
            _returnUrl.Item2,
            Is.EqualTo(true));
        return;

        Task<ActionExecutedContext> Next()
        {
            var filterMetadata = new List<IFilterMetadata>();
            var actionExecutedContext = new ActionExecutedContext(context, filterMetadata, Mock.Of<Controller>());
            return Task.FromResult(actionExecutedContext);
        }
    }

    [Test]
    public async Task WhenUsingLogRequestBodyActionFilter_NoExcludeArgumentsExistOnLogFilterAttribute_DoesNotThrowNullReferenceException()
    {
        IOptions<LogActionFilterConfig> config = Options.Create(
            new LogActionFilterConfig()
            {
                DisableBodyContent = false,
                ExcludeArguments = [],
                IncludeClaims = []
            });

        var actionDescriptor = new ActionDescriptor
        {
            FilterDescriptors =
            [
                new(
                    new LogFilterAttribute
                    {
                        ExcludeArguments = null, // This is the key to the test
                        MaxDepth = 5,
                        DisableBodyContent = false
                    },
                    FilterScope.Action)
            ]
        };

        var filterAttribute = new LogRequestBodyActionFilterAttribute(config);

        var context = GetExecutingContextWithActionArgs(actionDescriptor);

        Assert.That(context.HttpContext.Items, Has.Count.EqualTo(0));

        await filterAttribute.OnActionExecutionAsync(context, Next);

        var actionArgs = (RedactionDictionary)context.HttpContext.Items
            .Single(keyValuePair => keyValuePair.Key.ToString() == "ActionArguments").Value;

        Assert.That(actionArgs, Has.Count.EqualTo(2));
        Assert.That(
            actionArgs.Single(keyValuePair => keyValuePair.Key == _requestId.Item1).Value.ToString() ==
            _requestId.Item2,
            Is.EqualTo(true));
        Assert.That(
            actionArgs.Single(keyValuePair => keyValuePair.Key == _returnUrl.Item1).Value.ToString() ==
            _returnUrl.Item2,
            Is.EqualTo(true));
        return;

        Task<ActionExecutedContext> Next()
        {
            var filterMetadata = new List<IFilterMetadata>();
            var actionExecutedContext = new ActionExecutedContext(context, filterMetadata, Mock.Of<Controller>());
            return Task.FromResult(actionExecutedContext);
        }
    }

    private ActionExecutingContext GetExecutingContextWithActionArgs(ActionDescriptor? actionDescriptor = null)
    {
        var modelState = new ModelStateDictionary();

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Method = HttpMethod.Post.Method;

        actionDescriptor ??= new ActionDescriptor() { FilterDescriptors = [] };

        var filterMetadata = new List<IFilterMetadata>();
        var context = new ActionExecutingContext(
            new ActionContext(
                httpContext: httpContext,
                routeData: new RouteData(),
                actionDescriptor: actionDescriptor,
                modelState: modelState),
            filterMetadata,
            new Dictionary<string, object>()
                { { _returnUrl.Item1, _returnUrl.Item2 }, { _requestId.Item1, _requestId.Item2 } },
            new Mock<Controller>().Object);

        return context;
    }
}