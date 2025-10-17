using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class GodotServiceDefaults
{
	private static TracerProvider _tracerProvider = null!;
	private static MeterProvider _meterProvider = null!;
	public static void AddServiceDefaults()
	{
		var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
		if (endpoint is null)
		{
			Console.WriteLine("OTEL_EXPORTER_OTLP_ENDPOINT is not set, skipping OpenTelemetry setup.");
			return;
		}
		var endpointUri = new Uri(endpoint!);
		var resource = ResourceBuilder.CreateDefault()
			.AddService("sharpide-godot");

		_tracerProvider = Sdk.CreateTracerProviderBuilder()
			.SetResourceBuilder(resource)
			.AddSource("SharpIde")
			.AddOtlpExporter(options =>
			{
				options.Endpoint = endpointUri;
				options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
			})
			.Build();

		_meterProvider = Sdk.CreateMeterProviderBuilder()
			.SetResourceBuilder(resource)
			.AddMeter("SharpIde")
			.AddRuntimeInstrumentation()
			.AddOtlpExporter(options =>
			{
				options.Endpoint = endpointUri;
				options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
			})
			.Build();
	}
}
