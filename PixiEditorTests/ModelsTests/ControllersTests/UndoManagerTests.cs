using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class UndoManagerTests
    {

        public int ExampleProperty { get; set; } = 1;

        [Fact]
        public void TestSetRoot()
        {
            UndoManager.SetMainRoot(this);
            Assert.Equal(this, UndoManager.MainRoot);
        }

        [Fact]
        public void TestAddToUndoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            Assert.True(UndoManager.UndoStack.Count == 1);
            Assert.True((int)UndoManager.UndoStack.Peek().OldValue == ExampleProperty);
        }

        [Fact]
        public void TestThatUndoAddsToRedoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            Assert.True(UndoManager.RedoStack.Count == 1);
        }

        [Fact]
        public void TestUndo()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, 55));
            ExampleProperty = 55;
            UndoManager.Undo();
            Assert.True((int)UndoManager.RedoStack.Peek().OldValue == ExampleProperty);
        }


        [Fact]
        public void TestThatRedoAddsToUndoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.True(UndoManager.UndoStack.Count == 1);
        }

        [Fact]
        public void TestRedo()
        {
            PrepareUnoManagerForTests();
            ExampleProperty = 55;
            UndoManager.AddUndoChange(new Change("ExampleProperty", 1, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.True((int)UndoManager.UndoStack.Peek().NewValue == ExampleProperty);
        }

        private void PrepareUnoManagerForTests()
        {
            UndoManager.SetMainRoot(this);
            UndoManager.UndoStack.Clear();
            UndoManager.RedoStack.Clear();
            ExampleProperty = 1;
        }
    }
}
