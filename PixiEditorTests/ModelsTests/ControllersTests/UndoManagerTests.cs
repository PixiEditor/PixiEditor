using NUnit.Framework;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    [TestFixture]
    public class UndoManagerTests
    {

        public int ExampleProperty { get; set; } = 1;

        [TestCase]
        public void TestSetRoot()
        {
            UndoManager.SetMainRoot(this);
            Assert.AreEqual(this, UndoManager.MainRoot);
        }

        [TestCase]
        public void TestAddToUndoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            Assert.IsTrue(UndoManager.UndoStack.Count == 1);
            Assert.IsTrue((int)UndoManager.UndoStack.Peek().OldValue == ExampleProperty);
        }

        [TestCase]
        public void TestThatUndoAddsToRedoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            Assert.IsTrue(UndoManager.RedoStack.Count == 1);
        }

        [TestCase]
        public void TestUndo()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, 55));
            ExampleProperty = 55;
            UndoManager.Undo();
            Assert.IsTrue((int)UndoManager.RedoStack.Peek().OldValue == ExampleProperty);
        }


        [TestCase]
        public void TestThatRedoAddsToUndoStack()
        {
            PrepareUnoManagerForTests();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.IsTrue(UndoManager.UndoStack.Count == 1);
        }

        [TestCase]
        public void TestRedo()
        {
            PrepareUnoManagerForTests();
            ExampleProperty = 55;
            UndoManager.AddUndoChange(new Change("ExampleProperty", 1, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.IsTrue((int)UndoManager.UndoStack.Peek().NewValue == ExampleProperty);
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
