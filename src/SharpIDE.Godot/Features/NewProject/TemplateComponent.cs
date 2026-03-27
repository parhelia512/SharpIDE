using Godot;
using Microsoft.TemplateEngine.Abstractions;

namespace SharpIDE.Godot.Features.NewProject;

public partial class TemplateComponent : VBoxContainer
{
    private LineEdit _projectNameLineEdit = null!;
    private LineEdit _projectDirectoryLineEdit = null!;
    private Label _templateTypeLabel = null!;
    private ItemList _templatesItemList = null!;

    private Label _templateShortNameLabel = null!;
    private Label _templateIdentityLabel = null!;
    private Label _templateGroupIdentityLabel = null!;
    private Label _templateAuthorLabel = null!;
    private Label _templateClassificationsLabel = null!;
    
    private Button _createTemplateButton = null!;
    private Button _cancelButton = null!;

    private Dictionary<string, List<ITemplateInfo>> _templatesForCurrentCategory = null!;

    public override void _Ready()
    {
        _projectNameLineEdit = GetNode<LineEdit>("%ProjectNameLineEdit");
        _projectDirectoryLineEdit = GetNode<LineEdit>("%ProjectDirectoryLineEdit");
        _templateTypeLabel = GetNode<Label>("%TemplateTypeLabel");
        _templatesItemList = GetNode<ItemList>("%TemplatesItemList");
        
        _templateShortNameLabel = GetNode<Label>("%TemplateShortNameLabel");
        _templateIdentityLabel = GetNode<Label>("%TemplateIdentityLabel");
        _templateGroupIdentityLabel = GetNode<Label>("%TemplateGroupIdentityLabel");
        _templateAuthorLabel = GetNode<Label>("%TemplateAuthorLabel");
        _templateClassificationsLabel = GetNode<Label>("%TemplateClassificationsLabel");
        
        _createTemplateButton = GetNode<Button>("%CreateTemplateButton");
        _cancelButton = GetNode<Button>("%CancelButton");
        _cancelButton.Pressed += () =>
        {
            GetWindow().QueueFree();
        };
    }

    public void SetTemplates(Dictionary<string, List<ITemplateInfo>> templatesForCategory)
    {
        _templatesForCurrentCategory = templatesForCategory;
        var defaultTemplate = _templatesForCurrentCategory.First().Value.First();
        _projectNameLineEdit.Text = defaultTemplate.DefaultName;
        _templateTypeLabel.Text = defaultTemplate.Name;
        _templatesItemList.Clear();
        foreach (var template in templatesForCategory)
        {
            _templatesItemList.AddItem(template.Value.First().Name);
        }

        List<string> skippedParameterNames = ["TargetFrameworkOverride", "Framework", "langVersion", "skipRestore"]; 
        foreach (var parameter in defaultTemplate.ParameterDefinitions.Where(s => s.Precedence.PrecedenceDefinition is not PrecedenceDefinition.Implicit && skippedParameterNames.Contains(s.Name) is false))
        {
            ;
        }
        
        _templateShortNameLabel.Text = string.Join("; ", defaultTemplate.ShortNameList);
        _templateIdentityLabel.Text = defaultTemplate.Identity;
        _templateGroupIdentityLabel.Text = defaultTemplate.GroupIdentity;
        _templateAuthorLabel.Text = defaultTemplate.Author;
        _templateClassificationsLabel.Text = string.Join("; ", defaultTemplate.Classifications);
    }
}