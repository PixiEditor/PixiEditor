using System;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class UndoManagerTests
    {
        public int ExampleProperty { get; set; } = 1;
        public TestPropertyClass TestPropClass { get; set; } = new TestPropertyClass();

        public UndoManagerTests()
        {
            PrepareUndoManagerForTest();
        }

        [Fact]
        public void TestSetRoot()
        {
            PrepareUndoManagerForTest();
            UndoManager.SetMainRoot(null);
            UndoManager.SetMainRoot(this);
            Assert.Equal(this, UndoManager.MainRoot);
        }

        [Fact]
        public void TestAddToUndoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            Assert.True(UndoManager.UndoStack.Count == 1);
            Assert.True((int)UndoManager.UndoStack.Peek().OldValue == ExampleProperty);
        }

        [Fact]
        public void TestThatUndoAddsToRedoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            Assert.True(UndoManager.RedoStack.Count == 1);
        }

        [Fact]
        public void TestUndo()
        {
            PrepareUndoManagerForTest();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, 55));
            ExampleProperty = 55;
            UndoManager.Undo();
            Assert.True((int)UndoManager.RedoStack.Peek().OldValue == ExampleProperty);
        }


        [Fact]
        public void TestThatRedoAddsToUndoStack()
        {
            PrepareUndoManagerForTest();
            UndoManager.AddUndoChange(new Change("ExampleProperty", ExampleProperty, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.True(UndoManager.UndoStack.Count == 1);
        }

        [Fact]
        public void TestRedo()
        {
            PrepareUndoManagerForTest();
            ExampleProperty = 55;
            UndoManager.AddUndoChange(new Change("ExampleProperty", 1, ExampleProperty));
            UndoManager.Undo();
            UndoManager.Redo();
            Assert.True((int)UndoManager.UndoStack.Peek().NewValue == ExampleProperty);
        }

        [Fact]
        public void TestThatUndoManagerUndoAndRedoWithCustomRootCorrectly()
        {
            PrepareUndoManagerForTest();
            TestPropertyClass testProp = new TestPropertyClass();
            int newVal = 5;
            testProp.IntProperty = newVal;
            UndoManager.AddUndoChange(new Change("IntProperty", 0, newVal, root: testProp));
            Assert.Equal(newVal, testProp.IntProperty);
            
            UndoManager.Undo();

            Assert.Equal(0, testProp.IntProperty);

            UndoManager.Redo();

            Assert.Equal(newVal, testProp.IntProperty);
        }

        [Fact]
        public void TestThatMixedProcessOfUndoAndRedoWorks()
        {
            PrepareUndoManagerForTest();


            int newVal = 5;


            UndoManager.AddUndoChange(
                new Change("ExampleProperty",
                    ReverseProcess,
                    new object[]{ExampleProperty},
                    newVal));

            ExampleProperty = newVal;

            Assert.Equal(newVal, ExampleProperty);

            UndoManager.Undo();

            Assert.Equal(1, ExampleProperty);

            UndoManager.Redo();

            Assert.Equal(newVal, ExampleProperty);
        }

        [Fact]
        public void TestThatProcessBasedUndoAndRedoWorks()
        {
            PrepareUndoManagerForTest();
            int newVal = 5;
            UndoManager.AddUndoChange(new Change(ReverseProcess, new object[]{ExampleProperty}, ReverseProcess, 
                new object[]{newVal}));

            ExampleProperty = newVal;

            Assert.Equal(newVal, ExampleProperty);

            UndoManager.Undo();

            Assert.Equal(1, ExampleProperty);

            UndoManager.Redo();

            Assert.Equal(newVal, ExampleProperty);
        }

        [Fact]
        public void TestThatNestedPropertyUndoWorks()
        {
            PrepareUndoManagerForTest();
            int newVal = 5;

            UndoManager.AddUndoChange(new Change("TestPropClass.IntProperty", TestPropClass.IntProperty, 
                newVal));

            TestPropClass.IntProperty = newVal;

            Assert.Equal(newVal, TestPropClass.IntProperty);

            UndoManager.Undo();

            Assert.Equal(0, TestPropClass.IntProperty);

            UndoManager.Redo();

            Assert.Equal(newVal, TestPropClass.IntProperty);
        }

        private void ReverseProcess(object[] args)
        {
            ExampleProperty = (int)args[0];
        }

        private void PrepareUndoManagerForTest()
        {
            UndoManager.SetMainRoot(this);
            UndoManager.UndoStack.Clear();
            UndoManager.RedoStack.Clear();
            ExampleProperty = 1;
            TestPropClass = new TestPropertyClass {IntProperty = 0};
        }
    }

    public class TestPropertyClass
    {
        public int IntProperty { get; set; } = 0;
    }
}
