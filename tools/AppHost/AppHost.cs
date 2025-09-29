using AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//var photino = builder.AddProject<SharpIDE_Photino>("photino");

builder.AddGodot("../../src/SharpIDE.Godot/SharpIDE.Godot.csproj", "sharpide-godot")
	.WithOtlpExporter();

var appHost = builder.Build();

await appHost.RunAsync();
