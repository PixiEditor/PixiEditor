using Avalonia.Input;

namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class ToolAttribute : CommandAttribute
    {
        public Key Transient { get; set; }
        public bool TransientImmediate { get; set; } = false;

        public string? CommonToolType { get; set; }

        public ToolAttribute() : base(null, null, null)
        {
        }
    }
}
