using PixiEditor.ViewModels;
using System.Windows.Markup;

namespace PixiEditor.Models.Commands.XAML
{
    public class Command : MarkupExtension
    {
        private static CommandController commandController;

        public string Name { get; set; }

        public bool UseProvided { get; set; }

        public Command() { }

        public Command(string name) => Name = name;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (commandController == null)
            {
                commandController = ViewModelMain.Current.CommandController;
            }

            return commandController.Commands[Name].GetICommand(UseProvided);
        }
    }
}
