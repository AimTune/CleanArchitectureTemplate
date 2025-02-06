using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Constants;
using Shared.Settings;
using System.Diagnostics;

namespace Shared.Extensions;
public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryExtensions(this IServiceCollection services,
        IConfiguration configuration,
        Action<Activity, Exception>? aspnetEnrichWithException = default
    )
    {
        services.Configure<OpenTelemetrySettings>(configuration.GetSection("OpenTelemetry"));
        OpenTelemetrySettings otelSettings = (configuration.GetSection("OpenTelemetry").Get<OpenTelemetrySettings>())!;

        ActivitySourceProvider.Source = new ActivitySource(otelSettings.ActivitySourceName);

        services
            .AddOpenTelemetry()
            .WithTracing(options =>
            {
                options.AddSource(otelSettings.ActivitySourceName)
                    .AddSource(DiagnosticHeaders.DefaultListenerName) // For MassTransit
                    .ConfigureResource(
                        resource => resource.AddService(otelSettings.ServiceName, serviceVersion: otelSettings.ServiceVersion)
                            .AddAttributes([
                                new("host.machineName", Environment.MachineName),
                                new("host.environment", otelSettings.EnvironmentName)
                            ])
                    );

                options.AddAspNetCoreInstrumentationSettings(otelSettings, aspnetEnrichWithException);

                options.AddHttpClientInstrumentationSettings(otelSettings);

                if (otelSettings.ExporterSettings.ExportToConsole)
                {
                    options.AddConsoleExporter();
                }

                options.AddOtlpExporterSettings(otelSettings);
            });
    }
    public static void AddAspNetCoreInstrumentationSettings(this TracerProviderBuilder tracerProviderBuilder,
        OpenTelemetrySettings otelSettings,
        Action<Activity, Exception>? aspnetEnrichWithException)
    {
        if (!otelSettings.AspNetCoreInstrumentation.IsEnabled)
        {
            return;
        }

        tracerProviderBuilder.AddAspNetCoreInstrumentation(aspnetcoreOptions =>
        {
            aspnetcoreOptions.Filter = (context) =>
            {
                string[] paths = otelSettings.AspNetCoreInstrumentation.PathStartsWithExcludeFilters?.Split(';') ?? [];

                foreach (string path in paths)
                {
                    if (!string.IsNullOrEmpty(context.Request.Path.Value))
                    {
                        return !context.Request.Path.Value.StartsWith(path, StringComparison.InvariantCulture);
                    }
                }

                return true;
            };

            aspnetcoreOptions.RecordException = otelSettings.AspNetCoreInstrumentation.RecordException;
            aspnetcoreOptions.EnrichWithException = aspnetEnrichWithException;
        });
    }
    public static void AddHttpClientInstrumentationSettings(this TracerProviderBuilder tracerProviderBuilder, OpenTelemetrySettings otelSettings)
    {
        if (!otelSettings.HttpClientInstrumentation.IsEnabled)
        {
            return;
        }

        tracerProviderBuilder.AddHttpClientInstrumentation(httpOptions =>
        {
            httpOptions.FilterHttpRequestMessage = (request) => !request.RequestUri!.AbsoluteUri!.Contains("9200", StringComparison.InvariantCulture);

            httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
            {
                string requestContent = "empty";
                if (request.Content is not null && otelSettings.HttpClientInstrumentation.IncludeRequestBody)
                {
                    requestContent = await request.Content.ReadAsStringAsync();
                }
                activity.SetTag("http.request.body", requestContent);
            };

            httpOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
            {
                if (response.Content is not null && otelSettings.HttpClientInstrumentation.IncludeResponseBody)
                {
                    activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                }
            };
        });
    }
    public static void AddOtlpExporterSettings(this TracerProviderBuilder tracerProviderBuilder, OpenTelemetrySettings otelSettings)
    {
        if (otelSettings.ExporterSettings.OtelSettings.Uri is null)
        {
            return;
        }

        tracerProviderBuilder.AddOtlpExporter(otlpExporterConfigure =>
        {
            otlpExporterConfigure.Endpoint = new Uri(otelSettings.ExporterSettings.OtelSettings.Uri);

            otlpExporterConfigure.Protocol = otelSettings.ExporterSettings.OtelSettings.IsHttpProtobuf
                ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;

            otlpExporterConfigure.Headers = otelSettings.ExporterSettings.OtelSettings.Headers;
        });
    }
}
