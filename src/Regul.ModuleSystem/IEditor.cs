using PleasantUI.Controls.Custom;

namespace Regul.ModuleSystem
{
    public interface IEditor
    {
        string FileToPath { get; set; }

        Editor CurrentEditor { get; set; }

        Project CurrentProject { get; set; }

        string Id { get; set; }

        void Load(string path, Project project, PleasantTabItem pleasantTabItem, Editor editor);
        bool Save(string path);
        void Release();
    }
}