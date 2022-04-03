using PleasantUI.Controls.Custom;

namespace Regul.ModuleSystem;

public interface IEditor
{
    string? FilePath { get; set; }

    Editor Editor { get; set; }

    Project? Project { get; set; }

    string Id { get; set; }

    void Load(string? filePath, PleasantTabItem pleasantTabItem, Editor editor);
    bool Save(string path);
    void Release();
}