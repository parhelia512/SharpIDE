using Godot;

namespace SharpIDE.Godot.Features.About;

public partial class AutoUpdateComponent : VBoxContainer
{
	private HBoxContainer _checkForUpdatesHBoxContainer = null!;
	private Button _checkForUpdatesButton = null!;
	private Label _lastCheckedAtLabel = null!;
	
	private HBoxContainer _downloadUpdateHBoxContainer = null!;
	private Button _downloadUpdateButton = null!;
	
	private HBoxContainer _noUpdatesFoundHBoxContainer = null!;
	private HBoxContainer _checkingForUpdatesHBoxContainer = null!;
	private HBoxContainer _downloadingUpdateHBoxContainer = null!;
	
	private HBoxContainer _finishUpdateAndRestartHBoxContainer = null!;
	private Button _finishUpdateAndRestartButton = null!;
	
	private HBoxContainer _updatingAndRestartingHBoxContainer = null!;
	
	public override void _Ready()
	{
		_lastCheckedAtLabel = GetNode<Label>("%LastCheckedAtLabel");
		_checkForUpdatesHBoxContainer = GetNode<HBoxContainer>("%CheckForUpdatesHBoxContainer");
		_downloadUpdateHBoxContainer = GetNode<HBoxContainer>("%DownloadUpdateHBoxContainer");
		_noUpdatesFoundHBoxContainer = GetNode<HBoxContainer>("%NoUpdatesFoundHBoxContainer");
		_checkingForUpdatesHBoxContainer = GetNode<HBoxContainer>("%CheckingForUpdatesHBoxContainer");
		_downloadingUpdateHBoxContainer = GetNode<HBoxContainer>("%DownloadingUpdateHBoxContainer");
		_finishUpdateAndRestartHBoxContainer = GetNode<HBoxContainer>("%FinishUpdateAndRestartHBoxContainer");
		_updatingAndRestartingHBoxContainer = GetNode<HBoxContainer>("%UpdatingAndRestartingHBoxContainer");
		
		_checkForUpdatesButton = GetNode<Button>("%CheckForUpdatesButton");
		_downloadUpdateButton = GetNode<Button>("%DownloadUpdateButton");
		_finishUpdateAndRestartButton = GetNode<Button>("%FinishUpdateAndRestartButton");
		
		UpdateLastCheckedAtLabel();
	}

	private void UpdateLastCheckedAtLabel()
	{
		var lastChecked = Singletons.AppState.LastCheckedForUpdates;
		_lastCheckedAtLabel.Text = lastChecked is null
			? "Last checked at: never"
			: $"Last checked at: {lastChecked.Value.ToLocalTime():g}";
	}
}