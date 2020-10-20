using System;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class SelectionTests
    {
        [Fact]
        public void TestThatSetSelectionNewSetsCorrectSelection()
        {
            Selection selection = new Selection(Array.Empty<Coordinates>());
            Coordinates[] points = { new Coordinates(0, 0), new Coordinates(1, 1) };

            selection.SetSelection(points, SelectionType.New);
            selection.SetSelection(points, SelectionType.New); // Doing it twice, to check if it sets every time properly

            Assert.Equal(points.Length, selection.SelectedPoints.Count);
        }

        [Fact]
        public void TestThatSetSelectionAddSetsCorrectSelection()
        {
            Selection selection = new Selection(Array.Empty<Coordinates>());
            Coordinates[] points = { new Coordinates(0, 0), new Coordinates(1, 1) };
            Coordinates[] points2 = { new Coordinates(2, 4), new Coordinates(5, 7) };

            selection.SetSelection(points, SelectionType.Add);
            selection.SetSelection(points2, SelectionType.Add); // Doing it twice, to check if it sets every time properly

            Assert.Equal(points.Length + points2.Length, selection.SelectedPoints.Count);
        }

        [Fact]
        public void TestThatSetSelectionSubtractSetsCorrectSelection()
        {
            Selection selection = new Selection(Array.Empty<Coordinates>());
            Coordinates[] points = { new Coordinates(0, 0), new Coordinates(1, 1) };
            Coordinates[] points2 = { new Coordinates(1, 1) };

            selection.SetSelection(points, SelectionType.Add);
            selection.SetSelection(points2, SelectionType.Subtract); // Doing it twice, to check if it sets every time properly

            Assert.Single(selection.SelectedPoints);
        }

        [Fact]
        public void TestClearWorks()
        {
            Selection selection = new Selection(new[] { new Coordinates(0, 0), new Coordinates(5, 7) });
            selection.Clear();

            Assert.Empty(selection.SelectedPoints);
            Assert.Equal(0, selection.SelectionLayer.Width + selection.SelectionLayer.Height);
        }
    }
}