using System;
using System.Collections.Generic;
using System.Text;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    public class CoordinatesTests
    {

        [Fact]
        public void TestThatToStringReturnsCorrectFormat()
        {
            Coordinates cords = new Coordinates(5,5);

            Assert.Equal("5, 5",cords.ToString());
        }

        [Fact]
        public void TestThatNotEqualOperatorWorks()
        {
            Coordinates cords = new Coordinates(5,5);
            Coordinates cords2 = new Coordinates(6,4);

            Assert.True(cords != cords2);
        }

    }
}
