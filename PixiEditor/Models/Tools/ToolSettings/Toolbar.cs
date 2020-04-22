using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Toolbar
    {
        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        public virtual Setting GetSetting(string name)
        {
            return Settings.First(x => x.Name == name);
        }
    }
}
