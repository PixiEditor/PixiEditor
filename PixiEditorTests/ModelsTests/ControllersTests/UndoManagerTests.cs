using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Undo;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class UndoManagerTests
    {
        public UndoManagerTests()
        {
            PrepareUndoManagerForTest();
        }

        public int ExampleProperty { get; set; } = 1;

        public TestPropertyClass TestPropClass { get; set; } = new TestPropertyClass();

        [Fact]
        public void TestSetRoot()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);
            Assert.Equal(this, undoManager.MainRoot);
        }

        [Fact]
        public void TestAddToUndoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            Assert.True(undoManager.UndoStack.Count == 1);
            Assert.True((int)undoManager.UndoStack.Peek().OldValue == ExampleProperty);
        }

        [Fact]
        public void TestThatUndoAddsToRedoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            undoManager.Undo();
            Assert.True(undoManager.RedoStack.Count == 1);
        }

        [Fact]
        public void TestUndo()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, 55));
            ExampleProperty = 55;
            undoManager.Undo();
            Assert.True((int)undoManager.RedoStack.Peek().OldValue == ExampleProperty);
        }

        [Fact]
        public void TestThatRedoAddsToUndoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            undoManager.Undo();
            undoManager.Redo();
            Assert.True(undoManager.UndoStack.Count == 1);
        }

        [Fact]
        public void TestRedo()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            ExampleProperty = 55;
            undoManager.AddUndoChange(new Change("ExampleProperty", 1, ExampleProperty));
            undoManager.Undo();
            undoManager.Redo();
            Assert.True((int)undoManager.UndoStack.Peek().NewValue == ExampleProperty);
        }

        [Fact]
        public void TestThatUndoManagerUndoAndRedoWithCustomRootCorrectly()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            TestPropertyClass testProp = new TestPropertyClass();
            int newVal = 5;
            testProp.IntProperty = newVal;
            undoManager.AddUndoChange(new Change("IntProperty", 0, newVal, root: testProp));
            Assert.Equal(newVal, testProp.IntProperty);

            undoManager.Undo();

            Assert.Equal(0, testProp.IntProperty);

            undoManager.Redo();

            Assert.Equal(newVal, testProp.IntProperty);
        }

        [Fact]
        public void TestThatMixedProcessOfUndoAndRedoWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            int newVal = 5;

            undoManager.AddUndoChange(
                new Change(
                    "ExampleProperty",
                    ReverseProcess,
                    new object[] { ExampleProperty },
                    newVal));

            ExampleProperty = newVal;

            Assert.Equal(newVal, ExampleProperty);

            undoManager.Undo();

            Assert.Equal(1, ExampleProperty);

            undoManager.Redo();

            Assert.Equal(newVal, ExampleProperty);
        }

        [Fact]
        public void TestThatProcessBasedUndoAndRedoWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            int newVal = 5;
            undoManager.AddUndoChange(new Change(
                ReverseProcess,
                new object[] { ExampleProperty },
                ReverseProcess,
                new object[] { newVal }));

            ExampleProperty = newVal;

            Assert.Equal(newVal, ExampleProperty);

            undoManager.Undo();

            Assert.Equal(1, ExampleProperty);

            undoManager.Redo();

            Assert.Equal(newVal, ExampleProperty);
        }

        [Fact]
        public void TestThatNestedPropertyUndoWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            int newVal = 5;

            undoManager.AddUndoChange(new Change("TestPropClass.IntProperty", TestPropClass.IntProperty, newVal));

            TestPropClass.IntProperty = newVal;

            Assert.Equal(newVal, TestPropClass.IntProperty);

            undoManager.Undo();

            Assert.Equal(0, TestPropClass.IntProperty);

            undoManager.Redo();

            Assert.Equal(newVal, TestPropClass.IntProperty);
        }

        [Fact]
        public void TestThatFindRootProcessWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("IntProperty", 0, 5, FindRootProcess, null));

            Change change = undoManager.UndoStack.Peek();

            Assert.Equal(TestPropClass, change.FindRootProcess(change.FindRootProcessArgs));
        }

        [Fact]
        public void TestThatUndoForFindRootProcessWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("IntProperty", 0, 5, FindRootProcess, null));

            TestPropClass.IntProperty = 5;

            undoManager.Undo();

            Assert.Equal(0, TestPropClass.IntProperty);
        }

        [Fact]
        public void TestThatUndoAndRedoForFindRootProcessWorks()
        {
            PrepareUndoManagerForTest();
            UndoManager undoManager = new UndoManager(this);

            undoManager.AddUndoChange(new Change("IntProperty", 0, 5, FindRootProcess, null));

            TestPropClass.IntProperty = 5;

            undoManager.Undo();

            Assert.Equal(0, TestPropClass.IntProperty);

            undoManager.Redo();

            Assert.Equal(5, TestPropClass.IntProperty);
        }

        private object FindRootProcess(object[] args)
        {
            return TestPropClass;
        }

        private void ReverseProcess(object[] args)
        {
            ExampleProperty = (int)args[0];
        }

        private void PrepareUndoManagerForTest()
        {
            ExampleProperty = 1;
            TestPropClass = new TestPropertyClass { IntProperty = 0 };
        }
    }
}