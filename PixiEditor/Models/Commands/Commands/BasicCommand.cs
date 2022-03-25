using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands
{
    public partial class Command
    {
        public class BasicCommand : Command
        {
            public object Parameter { get; init; }

            protected override object GetParameter() => Parameter;
        }
    }
}
