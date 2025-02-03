using Shared.Attributes;
using System.Reflection;

namespace API.Extensions;

public static class OptionsExtensions
{
    public static void AddOptionsExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (Attribute.GetCustomAttribute(type, typeof(AddOptionsAttribute)) is AddOptionsAttribute attribute)
                {
                    MethodInfo configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                        .GetMethod("Configure", 1, [typeof(IServiceCollection), typeof(IConfiguration)])!
                        .MakeGenericMethod(type);

                    configureMethod.Invoke(null, [services, configuration.GetSection(attribute.OptionName)]);
                }
            }
        }
    }
}
