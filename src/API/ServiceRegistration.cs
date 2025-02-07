using API.Controllers;
using API.Extensions;
using Infrastructure.MessageBroker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Shared.Attributes;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace API;

public static class ServiceRegistration
{
    public const string CORS_NAME = "CORS_NAME";
    public const string MESSAGE_BROKER_SECTION_NAME = "MessageBroker";
    public const string CORS_SECTION_NAME = "CORS";
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection jwtConfig = configuration.GetSection("Jwt");
        byte[] key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

        services.AddScoped<TokenService>();

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

        services
            .AddCors(configuration)
            .AddAuthentication(configuration)
            .AddProblemDetails()
            .AddHttpContextAccessor()
            .AddEndpointsApiExplorer()
            .AddSettings(configuration);

        services.AddSignalR();
        // services.AddExceptionHandler<TransactionExceptionHandler>();
        services.AddControllers()
            .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new DynamicODataControllerFeatureProvider(services.BuildServiceProvider()));
            })
            .AddClenaArchitectureOData();
        // services.AddTransient<ExceptionHandlingMiddleware>();
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                });

        services.AddAuthorization();

        services.AddOpenApi(opt => opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
        services.AddHttpContextAccessor();
        services.AddAuthorizationBuilder();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }

    public static IEnumerable<Type> GetEntitiesWithODataAttribute()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<ODataEntityAttribute>() != null);
    }

    public static void AddClenaArchitectureOData(this IMvcBuilder mvcBuilder)
    {
        ODataConventionModelBuilder modelBuilder = new();

        Type[] types = typeof(Domain.AssemblyReference).Assembly.GetTypes();

        foreach (Type type in types)
        {
            ODataEntityAttribute attribute = (ODataEntityAttribute)Attribute.GetCustomAttribute(type, typeof(ODataEntityAttribute))!;

            if (attribute != null)
            {
                MethodInfo method = typeof(ODataConventionModelBuilder).GetMethod("EntitySet")!.MakeGenericMethod(type);
                method.Invoke(modelBuilder, [attribute.EntitySetName]);
            }
        }

        Microsoft.OData.Edm.IEdmModel edmModel = modelBuilder.GetEdmModel();

        mvcBuilder
            .AddOData(conf =>
            {
                conf.EnableQueryFeatures();
                conf.AddRouteComponents(routePrefix: "odata", model: edmModel);
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
            options
                .AddPolicy(
                    name: CORS_NAME,
                    policy => policy
                        .WithOrigins(configuration[CORS_SECTION_NAME]!.Split(";"))
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyHeader())
        );

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                options.Events = new JwtBearerEvents {
                    OnMessageReceived = context =>
                    {
                        Microsoft.Extensions.Primitives.StringValues accessToken = context.Request.Query["access_token"];
                        PathString path = context.HttpContext.Request.Path;
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}