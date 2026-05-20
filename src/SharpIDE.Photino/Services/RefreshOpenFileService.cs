namespace SharpIDE.Photino.Services;

public class RefreshOpenFileService
{
	public event Func<Task>? RefreshOpenFile;
	public void InvokeRefreshOpenFile()
	{
		_ = RefreshOpenFile?.Invoke();
	}
}
