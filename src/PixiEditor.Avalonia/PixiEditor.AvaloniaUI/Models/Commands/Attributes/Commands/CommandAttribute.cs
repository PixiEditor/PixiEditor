using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;

internal partial class Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    internal abstract class CommandAttribute : Attribute
    {
        public string InternalName { get; }

        public LocalizedString DisplayName { get; }

        public LocalizedString Description { get; }

        public string CanExecute { get; set; }

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
        /// Gets or sets path to the icon. Must be bitmap image
        /// </summary>
        public string IconPath { get; set; }

        protected CommandAttribute([InternalName] string internalName, string displayName, string description)
        {
            InternalName = internalName;
            DisplayName = displayName;
            Description = description;
        }

        public KeyCombination GetShortcut() => new() { Key = Key, Modifiers = Modifiers };
    }
}
