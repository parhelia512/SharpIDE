using Godot;

namespace SharpIDE.Godot.Features.Nuget;

public partial class NugetPanel : Control
{
    private VBoxContainer _installedPackagesVboxContainer = null!;
    private VBoxContainer _implicitlyInstalledPackagesItemList = null!;
    private VBoxContainer _availablePackagesItemList = null!;

    public override void _Ready()
    {
        _installedPackagesVboxContainer = GetNode<VBoxContainer>("%InstalledPackagesVBoxContainer");
        _implicitlyInstalledPackagesItemList = GetNode<VBoxContainer>("%ImplicitlyInstalledPackagesVBoxContainer");
        _availablePackagesItemList = GetNode<VBoxContainer>("%AvailablePackagesVBoxContainer");
    }
}