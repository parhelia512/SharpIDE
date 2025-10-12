using Godot;
using Microsoft.CodeAnalysis;

namespace SharpIDE.Godot.Features.CodeEditor;

public static partial class SymbolInfoComponents
{
    public static Control GetNamedTypeSymbolInfo(INamedTypeSymbol symbol)
    {
        var label = new RichTextLabel();
        label.FitContent = true;
        label.AutowrapMode = TextServer.AutowrapMode.Off;
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        label.PushColor(CachedColors.White);
        label.PushFont(MonospaceFont);
        label.AddAttributes(symbol);
        label.AddAccessibilityModifier(symbol);
        label.AddStaticModifier(symbol);
        label.AddVirtualModifier(symbol);
        label.AddAbstractModifier(symbol);
        label.AddOverrideModifier(symbol);
        label.AddNamedTypeSymbolName(symbol);
        label.AddInheritedTypes(symbol);
        label.AddContainingNamespaceAndClass(symbol);
        label.AddContainingPackage(symbol);
        label.Newline();
        label.Pop(); // font
        label.AddDocs(symbol);
        
        label.Pop();
        return label;
    }
    
    private static void AddNamedTypeSymbolName(this RichTextLabel label, INamedTypeSymbol symbol)
    {
        label.AddType(symbol);
    }
    
    private static void AddInheritedTypes(this RichTextLabel label, INamedTypeSymbol symbol)
    {
        if (symbol.BaseType is not null && symbol.BaseType.SpecialType != SpecialType.System_Object)
        {
            label.AddText(" : ");
            label.AddType(symbol.BaseType);
        }
        if (symbol.Interfaces.Length > 0)
        {
            if (symbol.BaseType is null || symbol.BaseType.SpecialType == SpecialType.System_Object)
            {
                label.AddText(" : ");
            }
            else
            {
                label.AddText(", ");
            }
            for (int i = 0; i < symbol.Interfaces.Length; i++)
            {
                var @interface = symbol.Interfaces[i];
                label.AddType(@interface);
                if (i < symbol.Interfaces.Length - 1)
                {
                    label.AddText(", ");
                }
            }
        }
    }
    
    private static void AddContainingPackage(this RichTextLabel label, INamedTypeSymbol symbol)
    {
        var containingModule = symbol.ContainingModule;
        if (containingModule is not null)
        {
            label.Newline();
            label.PushColor(CachedColors.White);
            label.AddText($"from module {containingModule.Name}");
            label.Pop();
        }
    }
}