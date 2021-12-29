using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.IO;

namespace Regul.SaveCleaner.Controls
{
    public class SaveFilePortrait : UserControl
    {
        private string _saveDir;

        public ulong ImgInstance { get; set; }

        public Image IconFamily;
        public TextBox SaveName;
        public TextBox SaveFamily;

        public string SaveDir
        {
            get => _saveDir;
            set
            {
                _saveDir = value;
                DirectoryInfo directoryInfo = new DirectoryInfo(value);
                string str = directoryInfo.ToString().Split('\\')[checked (directoryInfo.ToString().Split('\\').Length - 1)].Replace(".sims3", "");
                SaveName.Text = str;
            }
        }

        public string Location
        {
            get
            {
                string str = "";
                string[] files = Directory.GetFiles(SaveDir, "*.dat", SearchOption.AllDirectories);
                int index = 0;
                if (index < files.Length)
                    str = File.ReadAllText(files[index]).Split('_')[0];
                return str;
            }
        }

        public SaveFilePortrait()
        {
            InitializeComponent();
        }

        public SaveFilePortrait(string value, Save save)
        {
            InitializeComponent();

            IconFamily = this.FindControl<Image>("iconFamily");
            SaveName = this.FindControl<TextBox>("saveName");
            SaveFamily = this.FindControl<TextBox>("saveFamily");

            _saveDir = value;
            DirectoryInfo directoryInfo = new DirectoryInfo(value);
            string str = directoryInfo.ToString().Split('\\')[checked (directoryInfo.ToString().Split('\\').Length - 1)].Replace(".sims3", "");
            SaveName.Text = str;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            IconFamily = this.FindControl<Image>("iconFamily");
            SaveName = this.FindControl<TextBox>("saveName");
        }
    }
}
