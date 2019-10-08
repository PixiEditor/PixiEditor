using NUnit.Framework;
using PixiEditorDotNetCore3.Models;
using PixiEditorDotNetCore3.Models.Tools.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditorTests.WorkspaceTests.ToolsTests
{
    [TestFixture]
    public class CoordinatesCalculatorTests
    {
        [TestCase(0,0,2,2, ExpectedResult = 9)]
        [TestCase(0,0,10,10, ExpectedResult = 121)]
        public int RectangleToCoordinatesAmountTest(int x1, int y1, int x2, int y2)
        {
            return CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2).Length;
        }
    }
}
