namespace Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AddOptionsAttribute(string propertyName) : Attribute
{
    public string OptionName { get; } = propertyName;
}