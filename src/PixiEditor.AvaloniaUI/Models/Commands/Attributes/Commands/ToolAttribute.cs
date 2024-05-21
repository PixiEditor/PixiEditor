using Avalonia.Input;

namespace PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;

internal partial class Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class ToolAttribute : CommandAttribute
    {
        public Key Transient { get; set; }

        public ToolAttribute() : base(null, null, null)
        {
        }
    }
}
