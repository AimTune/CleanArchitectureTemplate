namespace Shared.Settings;
public class OpenTelemetrySettings
{
    public string ServiceName { get; set; } = null!;
    public string ServiceVersion { get; set; } = null!;
    public string EnvironmentName { get; set; } = "Production";
    public string ActivitySourceName { get; set; } = null!;
    public AspNetCoreInstrumentationSettings AspNetCoreInstrumentation { get; set; } = new();
    public HttpClientInstrumentationSettings HttpClientInstrumentation { get; set; } = new();
    public OpenTelemetryExporterSettings ExporterSettings { get; set; } = new();
}

public class OpenTelemetryExporterSettings
{
    public bool ExportToConsole { get; set; }
    public OtelSettings OtelSettings { get; set; } = new();
}

public class OtelSettings
{
    public string? Uri { get; set; }
    public bool IsHttpProtobuf { get; set; }
    public string Headers { get; set; } = string.Empty;
}

public class AspNetCoreInstrumentationSettings
{
    public bool IsEnabled { get; set; } = true;
    public string PathStartsWithExcludeFilters { get; set; } = "swagger;scalar";
    public bool RecordException { get; set; }
}

public class HttpClientInstrumentationSettings
{
    public bool IsEnabled { get; set; } = true;
    public bool IncludeRequestBody { get; set; } = true;
    public bool IncludeResponseBody { get; set; } = true;
}
