using Godot;
using SharpIDE.Application.Features.Nuget;

namespace SharpIDE.Godot.Features.Nuget;

public partial class PackageEntry : MarginContainer
{
    private Label _packageNameLabel = null!;
    private Label _currentVersionLabel = null!;
    private Label _latestVersionLabel = null!;
    private HBoxContainer _sourceNamesContainer = null!;
    private TextureRect _packageIconTextureRect = null!;
    
    private static readonly Color Source_NugetOrg_Color = new Color("629655");
    private static readonly Color Source_2_Color = new Color("008989");
    private static readonly Color Source_3_Color = new Color("8d75a8");
    private static readonly Color Source_4_Color = new Color("966a00");
    private static readonly Color Source_5_Color = new Color("efaeae");
    
    public IdePackageResult PackageResult { get; set; } = null!;
    public override void _Ready()
    {
        _packageNameLabel = GetNode<Label>("%PackageNameLabel");
        _currentVersionLabel = GetNode<Label>("%CurrentVersionLabel");
        _latestVersionLabel = GetNode<Label>("%LatestVersionLabel");
        _sourceNamesContainer = GetNode<HBoxContainer>("%SourceNamesHBoxContainer");
        _packageIconTextureRect = GetNode<TextureRect>("%PackageIconTextureRect");
        ApplyValues();
    }
    
    private void ApplyValues()
    {
        if (PackageResult is null) return;
        _packageNameLabel.Text = PackageResult.PackageSearchMetadata.Identity.Id;
        _currentVersionLabel.Text = string.Empty;
        //_latestVersionLabel.Text = $"Latest: {PackageResult.PackageSearchMetadata.vers.LatestVersion}";
        _sourceNamesContainer.QueueFreeChildren();
        
        var iconUrl = PackageResult.PackageSearchMetadata.IconUrl;
        if (iconUrl != null)
        {
            var httpRequest = new HttpRequest(); // Godot's abstraction
            AddChild(httpRequest);
            httpRequest.RequestCompleted += (result, responseCode, headers, body) =>
            {
                if (responseCode is 200)
                {
                    var image = new Image();
                    image.LoadPngFromBuffer(body);
                    image.Resize(32, 32, Image.Interpolation.Lanczos);
                    var loadedImageTexture = ImageTexture.CreateFromImage(image);
                    _packageIconTextureRect.Texture = loadedImageTexture;
                }
                httpRequest.QueueFree();
            };
            httpRequest.Request(iconUrl.ToString());
        }
        
        foreach (var source in PackageResult.PackageSources)
        {
            var label = new Label { Text = source.Name };
            label.AddThemeColorOverride("font_color", source.Name switch
            {
                // TODO: Make dynamic
                "nuget.org" => Source_NugetOrg_Color,
                "Microsoft Visual Studio Offline Packages" => Source_2_Color,
                _ => Source_3_Color
            });
            _sourceNamesContainer.AddChild(label);
        }
    }
}