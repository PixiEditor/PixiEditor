using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using ActualCommand = PixiEditor.Models.Commands.Command;

namespace PixiEditor.Models.Commands.XAML
{
    public class ShortcutBinding : MarkupExtension
    {
        private static CommandController commandController;

        public string Name { get; set; }

        public ShortcutBinding() { }

        public ShortcutBinding(string name) => Name = name;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (commandController == null)
            {
                commandController = ViewModelMain.Current.CommandController;
            }

            return GetBinding(commandController.Commands[Name]).ProvideValue(serviceProvider);
        }

        public static Binding GetBinding(ActualCommand command) => new Binding
        {
            Source = command,
            Path = new("Shortcut"),
            Mode = BindingMode.OneWay,
            StringFormat = ""
        };
    }
}
