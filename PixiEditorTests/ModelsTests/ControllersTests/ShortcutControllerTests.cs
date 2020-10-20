using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.Shortcuts;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class ShortcutControllerTests
    {
        private static ShortcutController GenerateStandardShortcutController(Key shortcutKey, ModifierKeys modifiers, RelayCommand shortcutCommand)
        {
            var controller = new ShortcutController();
            controller.Shortcuts.Add(new Shortcut(shortcutKey, shortcutCommand, 0, modifiers));
            ShortcutController.BlockShortcutExecution = false;
            return controller;
        }

        [StaTheory]
        [InlineData(Key.A, ModifierKeys.None, Key.A, ModifierKeys.None)]
        [InlineData(Key.A, ModifierKeys.Alt, Key.A, ModifierKeys.Alt)]
        [InlineData(Key.B, ModifierKeys.Alt | ModifierKeys.Control, Key.B, ModifierKeys.Alt | ModifierKeys.Control)]
        public void TestThatShortcutControllerExecutesShortcut(Key shortcutKey, ModifierKeys shortcutModifiers, Key clickedKey, ModifierKeys clickedModifiers)
        {
            var result = -1;
            var shortcutCommand = new RelayCommand(arg => { result = (int) arg; });
            var controller = GenerateStandardShortcutController(shortcutKey, shortcutModifiers, shortcutCommand);

            controller.KeyPressed(clickedKey, clickedModifiers);
            Assert.Equal(0, result);
        }

        [StaTheory]
        [InlineData(Key.B, ModifierKeys.None, Key.A, ModifierKeys.None)]
        [InlineData(Key.A, ModifierKeys.Alt, Key.A, ModifierKeys.None)]
        [InlineData(Key.C, ModifierKeys.Alt | ModifierKeys.Control, Key.C, ModifierKeys.Alt | ModifierKeys.Windows)]
        public void TestThatShortcutControllerNotExecutesShortcut(Key shortcutKey, ModifierKeys shortcutModifiers, Key clickedKey, ModifierKeys clickedModifiers)
        {
            var result = -1;
            var shortcutCommand = new RelayCommand(arg => { result = (int) arg; });
            var controller = GenerateStandardShortcutController(shortcutKey, shortcutModifiers, shortcutCommand);


            controller.KeyPressed(clickedKey, clickedModifiers);
            Assert.Equal(-1, result);
        }

        [StaFact]
        public void TestThatShortcutControllerIsBlocked()
        {
            var result = -1;
            var shortcutCommand = new RelayCommand(arg => { result = (int) arg; });

            var controller = GenerateStandardShortcutController(Key.A, ModifierKeys.None, shortcutCommand);
            ShortcutController.BlockShortcutExecution = true;

            controller.KeyPressed(Key.A, ModifierKeys.None);
            Assert.Equal(-1, result);
        }

        [StaFact]
        public void TestThatShortcutControllerPicksCorrectShortcut()
        {
            var result = -1;
            var shortcutCommand = new RelayCommand(arg => { result = (int) arg; });

            var controller = GenerateStandardShortcutController(Key.A, ModifierKeys.None, shortcutCommand);
            controller.Shortcuts.Add(new Shortcut(Key.A, shortcutCommand, 1, ModifierKeys.Control));

            controller.KeyPressed(Key.A, ModifierKeys.Control);
            Assert.Equal(1, result);
        }

        [StaFact]
        public void TestThatKeyPressedSetsLastShortcut()
        {
            var controller = GenerateStandardShortcutController(Key.A, ModifierKeys.None,
                new RelayCommand(parameter => { }));

            Assert.Null(controller.LastShortcut);
            controller.KeyPressed(Key.A, ModifierKeys.None);
            Assert.Equal(controller.Shortcuts[0], controller.LastShortcut);
        }
    }
}