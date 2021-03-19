using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Controllers.Shortcuts
{
    public class ShortcutGroup
    {
        /// <summary>
        /// Gets or sets the shortcuts in the shortcuts group.
        /// </summary>
        public ObservableCollection<Shortcut> Shortcuts { get; set; }

        /// <summary>
        /// Gets or sets the name of the shortcut group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the shortcut group visible in the shortcut popup.
        /// </summary>
        public bool IsVisible { get; set; }

        public ShortcutGroup(string name, params Shortcut[] shortcuts)
        {
            Name = name;
            Shortcuts = new ObservableCollection<Shortcut>(shortcuts);
            IsVisible = true;
        }

        /// <param name="name">The name of the group.</param>
        /// <param name="shortcuts">The shortcuts that belong in the group.</param>
        /// <param name="isVisible">Is the group visible in the shortcut popup.</param>
        public ShortcutGroup(string name, bool isVisible = true, params Shortcut[] shortcuts)
            : this(name, shortcuts)
        {
            IsVisible = isVisible;
        }
    }
}