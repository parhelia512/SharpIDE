using System.Diagnostics;
using Microsoft.Build.Locator;

namespace SharpIDE.MsBuildHost;

public static class SharpIdeMsbuildLocator
{
	public static string ResolvedMsBuildSdkPath { get; private set; } = null!;
	public static void Register(string sdkVersion)
	{
		if (OperatingSystem.IsMacOS())
		{
			FixMacosPath();
		}
		// Use latest version - https://github.com/microsoft/MSBuildLocator/issues/81
		var instance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault(s => s.MSBuildPath.EndsWith(sdkVersion, StringComparison.OrdinalIgnoreCase));
		if (instance is null) throw new InvalidOperationException("No matching MSBuild instances found");
		MSBuildLocator.RegisterInstance(instance);
		ResolvedMsBuildSdkPath = instance.MSBuildPath;
	}

	// https://github.com/microsoft/MSBuildLocator/issues/361
	private static void FixMacosPath()
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = "/bin/zsh",
			ArgumentList = { "-l", "-c", "printenv PATH" },
			RedirectStandardOutput = true,
			RedirectStandardError =  true,
			UseShellExecute = false,
		};
		using var process = Process.Start(processStartInfo);
		var output = process!.StandardOutput.ReadToEnd().Trim();
		process.WaitForExit();
		Environment.SetEnvironmentVariable("PATH", output, EnvironmentVariableTarget.Process);
	}
}
