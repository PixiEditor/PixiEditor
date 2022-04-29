using System.Collections;

namespace PixiEditor.Models.Commands
{
    public class CommandGroup : IEnumerable<Command>
    {
        private readonly Command[] commands;
        private readonly Command[] visibleCommands;

        public string Display { get; set; }

        public IEnumerable<Command> Commands => commands;

        public IEnumerable<Command> VisibleCommands => visibleCommands;

        public CommandGroup(string display, IEnumerable<Command> commands)
        {
            Display = display;
            this.commands = commands.ToArray();
            visibleCommands = commands.Where(x => !string.IsNullOrEmpty(x.Display)).ToArray();
        }

        public IEnumerator<Command> GetEnumerator() => Commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Commands.GetEnumerator();
    }
}
