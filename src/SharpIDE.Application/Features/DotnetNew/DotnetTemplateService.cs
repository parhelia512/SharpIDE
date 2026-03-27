using Microsoft.DotNet.Cli.Commands.New;
using Microsoft.Extensions.Logging;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.TemplatePackage;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.IDE;

namespace SharpIDE.Application.Features.DotnetNew;

public class DotnetTemplateService(ILoggerFactory loggerFactory)
{
	private readonly ILoggerFactory _loggerFactory = loggerFactory;
	public static HashSet<string> MicrosoftTemplateCategories { get; } = new HashSet<string>(["Console", "Library", "Web", "WinForms", "WPF", "Test", "Aspire"], StringComparer.OrdinalIgnoreCase);
	private CliTemplateEngineHost? _templateEngineHost;
	private Bootstrapper? _bootstrapper;

	public async Task<Dictionary<string, Dictionary<string, List<ITemplateInfo>>>> GetCategorisedTemplates(CancellationToken cancellationToken = default)
	{
		var templates = await GetTemplates(cancellationToken);
		var categories = MicrosoftTemplateCategories;

		var categorizedTemplates = templates
			.Where(t =>
				t.TagsCollection.GetValueOrDefault("type") == "project" &&
				t.TagsCollection.GetValueOrDefault("language") == "C#")
			.Select(t => new
			{
				Template = t,
				Category = t.Author == "Microsoft"
					? t.Classifications.FirstOrDefault(c => categories.Contains(c)) ?? "Custom"
					: "Custom"
			});

		var result = categorizedTemplates
			.GroupBy(x => x.Category)
			.ToDictionary(
				g => g.Key,
				g => g.GroupBy(x => x.Template.GroupIdentity ?? x.Template.Identity)
					.ToDictionary(
						gg => gg.Key,
						gg => gg.Select(x => x.Template).ToList()
					)
			);
		return result;
	}

	public async Task<IReadOnlyList<ITemplateInfo>> GetTemplates(CancellationToken cancellationToken = default)
	{
		_templateEngineHost ??= CliTemplateEngineHost.CreateHost(false, false, null, null, false, LogLevel.Information, _loggerFactory);
		_bootstrapper ??= new Bootstrapper(_templateEngineHost, false);
		var templates = await _bootstrapper.GetTemplatesAsync(cancellationToken);

		return templates;

		// Console.WriteLine($"Found {templates.Count} templates");
		// foreach (var template in templates)
		// {
		// 	Console.WriteLine($"Template package: {template.Name} ({template.Identity})");
		// }

		// run a template
		// var template = templates.First();
		// var path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\SharpIdeTestApp";
		// var name = Path.GetFileName(path);
		// var templateCreator = await bootstrapper.CreateAsync(template, name, path, (Dictionary<string, string?>)[], null, cancellationToken);
		;
	}

	/// <summary>
	/// <paramref name="path"/> must include the project folder, ie the <paramref name="projectName"/> is not appended to it by the Template Engine
	/// </summary>
	public async Task ExecuteTemplate(ITemplateInfo template, string projectName, string path, Dictionary<string, string?> parameters, CancellationToken cancellationToken = default)
	{
		var templateCreationResult = await _bootstrapper!.CreateAsync(template, projectName, path, parameters, null, cancellationToken);
	}
}
