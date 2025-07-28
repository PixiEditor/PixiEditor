using Avalonia.Input;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    internal abstract class CommandAttribute : Attribute
    {
        public string InternalName { get; }

        public LocalizedString DisplayName { get; }

        public LocalizedString Description { get; }

        public string CanExecute { get; set; }
        
        public Type[]? ShortcutContexts { get; set; }

        /// <summary>
        /// Gets or sets the default shortcut key for this command
        /// </summary>
        public Key Key { get; set; }

        /// <summary>
        /// Gets or sets the default shortcut modfiers keys for this command
        /// </summary>
        public KeyModifiers Modifiers { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the icon evaluator for this command
        /// </summary>
        public string IconEvaluator { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// Gets or sets whether this command should be tracked in analytics.
        /// </summary>
        public bool AnalyticsTrack { get; set; }

        /// <summary>
        ///     Gets or sets the path to the menu item. If null, command will not be added to menu.
        /// </summary>
        public string? MenuItemPath { get; set; }

        /// <summary>
        ///     Gets or sets the order of the menu item. By default, order is 100, so commands are added to the end of the menu.
        /// </summary>
        public int MenuItemOrder { get; set; } = 100;

        protected CommandAttribute([InternalName] string internalName, string displayName, string description)
        {
            InternalName = internalName;
            DisplayName = displayName;
            Description = description;
        }

        public KeyCombination GetShortcut() => new() { Key = Key, Modifiers = Modifiers };
    }
}
