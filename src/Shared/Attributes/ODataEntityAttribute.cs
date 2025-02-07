namespace Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ODataEntityAttribute(string entitySetName) : Attribute
{
    public string EntitySetName { get; } = entitySetName;
}