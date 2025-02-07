using MassTransit;

namespace Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AddEventConsumerAttribute : Attribute
{
    public AddEventConsumerAttribute() { }
    public AddEventConsumerAttribute(Type consumerDefinitionType)
    {
        if (!IsGenericConsumerDefinition(consumerDefinitionType))
        {
            throw new ArgumentException("The type must inherit from ConsumerDefinition<>.", nameof(consumerDefinitionType));
        }

        ConsumerDefinitionType = consumerDefinitionType;
    }
    public Type? ConsumerDefinitionType { get; }

    private static bool IsGenericConsumerDefinition(Type type)
    {
        // Taban türleri kontrol et ve ConsumerDefinition<> türevini ara
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConsumerDefinition<>))
            {
                return true;
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            type = type.BaseType ?? null; // BaseType null olabilir, bu yüzden kontrol eklendi
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        return false;
    }
}