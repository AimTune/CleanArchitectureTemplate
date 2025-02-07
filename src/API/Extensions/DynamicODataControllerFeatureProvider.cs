using Application.Abstractions.DbContexts;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Shared.Attributes;
using System.Reflection;
using System.Reflection.Emit;

namespace API.Extensions;

public class DynamicODataControllerFeatureProvider(IServiceProvider serviceProvider)
    : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        IEnumerable<Type> entities = ServiceRegistration.GetEntitiesWithODataAttribute();

        foreach (Type entity in entities)
        {
            ODataEntityAttribute? attribute = entity.GetCustomAttribute<ODataEntityAttribute>();
            if (attribute == null) continue;

            string controllerName = $"{attribute.EntitySetName}Controller";

            if (feature.Controllers.Any(c => c.Name == controllerName)) continue;

            (bool iscompleted, TypeBuilder typeBuilder) = CreateControllerType(controllerName, entity);

            if (iscompleted)
            {
                feature.Controllers.Add(typeBuilder.CreateTypeInfo());
            }
        }
    }

    private (bool, TypeBuilder) CreateControllerType(string controllerName, Type entityType)
    {
        bool isCompleted = false;
        AssemblyName assemblyName = new("DynamicODataControllers");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        TypeBuilder typeBuilder = moduleBuilder.DefineType(
            controllerName,
            TypeAttributes.Public | TypeAttributes.Class,
            typeof(ODataBaseController<>).MakeGenericType(entityType)
        );

        // Constructor ekle
        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [typeof(IApplicationDbContext)]
        );

        ILGenerator ilGenerator = constructorBuilder.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        ConstructorInfo? baseCtor = typeof(ODataBaseController<>)
            .MakeGenericType(entityType)
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(IApplicationDbContext)],
                null
            );

        if (baseCtor != null)
        {
            ilGenerator.Emit(OpCodes.Call, baseCtor);
            ilGenerator.Emit(OpCodes.Ret);
            isCompleted = true;
        }

        return (isCompleted, typeBuilder);
    }
}