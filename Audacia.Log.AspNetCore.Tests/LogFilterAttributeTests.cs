namespace Audacia.Log.AspNetCore.Tests;

public class LogFilterAttributeTests
{
    [Test]
    public void LogFilterAttribute_ExcludeArgumentsAndIncludeClaims_MustBeValidAttributeParameterType()
    {
        var attributeType = typeof(LogFilterAttribute);

        var excludeArgumentsProperty = attributeType.GetProperty(nameof(LogFilterAttribute.ExcludeArguments));
        var includeClaimsProperty = attributeType.GetProperty(nameof(LogFilterAttribute.IncludeClaims));

        Assert.That(excludeArgumentsProperty, Is.Not.Null);
        Assert.That(includeClaimsProperty, Is.Not.Null);

        Type[] validAttributeParameterTypes =
        [
            typeof(string),
            typeof(int),
            typeof(double),
            typeof(bool),
            typeof(Enum),
            typeof(string[])
        ];

        Assert.That(
            excludeArgumentsProperty.PropertyType,
            Is.AnyOf(validAttributeParameterTypes),
            "ExcludeArguments must be a valid attribute parameter type.");

        Assert.That(
            includeClaimsProperty.PropertyType,
            Is.AnyOf(validAttributeParameterTypes),
            "IncludeClaims must be a valid attribute parameter type.");
    }
}
