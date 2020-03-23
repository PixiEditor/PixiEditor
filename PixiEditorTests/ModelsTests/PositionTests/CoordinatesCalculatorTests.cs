using NUnit.Framework;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    [TestFixture]
    public class CoordinatesCalculatorTests
    {
        [TestCase(5,5, 1)]
        public void TestCenterOfThickness(int x1, int y1, int thickness)
        {
            CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(x1, y1), thickness);
            
        }

    }
}
