using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace PleasantUI.Xaml.Interactivity;

public class BehaviorCollectionTemplate : ITemplate
{
    [Content]
    [TemplateContent(TemplateResultType = typeof(BehaviorCollection))]
    public object? Content { get; set; }

    object ITemplate.Build() => TemplateContent.Load<BehaviorCollection>(Content).Result;
}