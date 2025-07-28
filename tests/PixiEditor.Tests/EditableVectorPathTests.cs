using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using Drawie.Skia;
using DrawiEngine;
using PixiEditor.Views.Overlays.PathOverlay;

namespace PixiEditor.Tests;

public class EditableVectorPathTests : PixiEditorTest
{
    [Fact]
    public void TestThatRectVectorShapeReturnsCorrectSubShapes()
    {
        VectorPath path = new VectorPath();
        path.AddRect(new RectD(0, 0, 10, 10));

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Single(editablePath.SubShapes);
        Assert.True(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(4, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(10, 0), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(10, 10), editablePath.SubShapes[0].Points[2].Position);
        Assert.Equal(new VecF(0, 10), editablePath.SubShapes[0].Points[3].Position);
    }
    
    [Fact]
    public void TestThatOvalVectorShapeReturnsCorrectSubShapes()
    {
        VectorPath path = new VectorPath();
        path.AddOval(RectD.FromCenterAndSize(new VecD(5, 5), new VecD(10, 10)));

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Single(editablePath.SubShapes);
        Assert.True(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(4, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(10, 5), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(5, 10), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(0, 5), editablePath.SubShapes[0].Points[2].Position);
        Assert.Equal(new VecF(5, 0), editablePath.SubShapes[0].Points[3].Position);
    }
    
    [Fact]
    public void TestThatLineVectorShapeReturnsCorrectSubShapes()
    {
        VectorPath path = new VectorPath();
        path.LineTo(new VecF(2, 2));
        path.Close();

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Single(editablePath.SubShapes);
        Assert.True(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(2, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[1].Position);
    }
    
    [Fact]
    public void TestThatNotClosedPolyReturnsCorrectSubShape()
    {
        VectorPath path = new VectorPath();
        
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Single(editablePath.SubShapes);
        Assert.False(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(3, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(4, 4), editablePath.SubShapes[0].Points[2].Position);
    }
    
    [Fact]
    public void TestThatMultipleRectsReturnCorrectSubShapes()
    {
        VectorPath path = new VectorPath();
        path.AddRect(new RectD(0, 0, 10, 10));
        path.AddRect(new RectD(10, 10, 20, 20));

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Equal(2, editablePath.SubShapes.Count);
        
        Assert.True(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(4, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(10, 0), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(10, 10), editablePath.SubShapes[0].Points[2].Position);
        Assert.Equal(new VecF(0, 10), editablePath.SubShapes[0].Points[3].Position);
        
        Assert.True(editablePath.SubShapes[1].IsClosed);
        Assert.Equal(4, editablePath.SubShapes[1].Points.Count);
        
        Assert.Equal(new VecF(10, 10), editablePath.SubShapes[1].Points[0].Position);
        Assert.Equal(new VecF(30, 10), editablePath.SubShapes[1].Points[1].Position);
        Assert.Equal(new VecF(30, 30), editablePath.SubShapes[1].Points[2].Position);
        Assert.Equal(new VecF(10, 30), editablePath.SubShapes[1].Points[3].Position);
    }

    [Fact]
    public void TestThatTwoPolysWithSecondUnclosedReturnsCorrectShapeData()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();
        
        path.MoveTo(new VecF(10, 10));
        path.LineTo(new VecF(12, 12));
        path.LineTo(new VecF(14, 14));

        EditableVectorPath editablePath = new EditableVectorPath(path);

        Assert.Equal(2, editablePath.SubShapes.Count);
        
        Assert.True(editablePath.SubShapes[0].IsClosed);
        Assert.Equal(3, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(4, 4), editablePath.SubShapes[0].Points[2].Position);
        
        Assert.False(editablePath.SubShapes[1].IsClosed);
        Assert.Equal(3, editablePath.SubShapes[1].Points.Count);
        
        Assert.Equal(new VecF(10, 10), editablePath.SubShapes[1].Points[0].Position);
        Assert.Equal(new VecF(12, 12), editablePath.SubShapes[1].Points[1].Position);
        Assert.Equal(new VecF(14, 14), editablePath.SubShapes[1].Points[2].Position);
    }

    [Fact]
    public void TestThatStartAndEndIndexesForMultipleShapesAreCorrect()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();
        
        path.MoveTo(new VecF(10, 10));
        path.LineTo(new VecF(12, 12));
        path.Close();
        
        int expectedFirstShapeStartIndex = 0;
        int expectedFirstShapeEndIndex = 2;
        
        int expectedSecondShapeStartIndex = 3;
        int expectedSecondShapeEndIndex = 5;
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(2, editablePath.SubShapes.Count);
    }
    
    [Fact]
    public void TestThatGetNextPointInTriangleShapeReturnsCorrectPoint()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();

        EditableVectorPath editablePath = new EditableVectorPath(path);

        ShapePoint nextPoint = editablePath.SubShapes[0].GetNextPoint(0);
        
        Assert.Equal(new VecF(2, 2), nextPoint.Position);
        Assert.Equal(1, nextPoint.Index);
        
        nextPoint = editablePath.SubShapes[0].GetNextPoint(1);
        
        Assert.Equal(new VecF(4, 4), nextPoint.Position);
        Assert.Equal(2, nextPoint.Index);
        
        nextPoint = editablePath.SubShapes[0].GetNextPoint(2);
        
        Assert.Equal(new VecF(0, 0), nextPoint.Position);
        Assert.Equal(0, nextPoint.Index);
    }
    
    [Fact]
    public void TestThatGetPreviousPointInTriangleShapeReturnsCorrectPoint()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();

        EditableVectorPath editablePath = new EditableVectorPath(path);

        ShapePoint previousPoint = editablePath.SubShapes[0].GetPreviousPoint(0);
        
        Assert.Equal(new VecF(4, 4), previousPoint.Position);
        Assert.Equal(2, previousPoint.Index);
        
        previousPoint = editablePath.SubShapes[0].GetPreviousPoint(1);
        
        Assert.Equal(new VecF(0, 0), previousPoint.Position);
        Assert.Equal(0, previousPoint.Index);
        
        previousPoint = editablePath.SubShapes[0].GetPreviousPoint(2);
        
        Assert.Equal(new VecF(2, 2), previousPoint.Position);
        Assert.Equal(1, previousPoint.Index);
    }

    [Fact]
    public void TestThatVerbsInTriangleAreCorrect()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(3, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(PathVerb.Line, editablePath.SubShapes[0].Points[0].Verb.VerbType);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Verb.From);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[0].Verb.To);
        
        Assert.Equal(PathVerb.Line, editablePath.SubShapes[0].Points[1].Verb.VerbType);
        
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[1].Verb.From);
        Assert.Equal(new VecF(4, 4), editablePath.SubShapes[0].Points[1].Verb.To);
        
        Assert.Equal(PathVerb.Line, editablePath.SubShapes[0].Points[2].Verb.VerbType);
        
        Assert.Equal(new VecF(4, 4), editablePath.SubShapes[0].Points[2].Verb.From);
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[2].Verb.To);
    }

    [Fact]
    public void TestThatVerbsInOvalAreCorrect()
    {
        const float conic = 0.70710769f;
        const float rangeLower = conic - 0.001f;
        const float rangeUpper = conic + 0.001f;
        VectorPath path = new VectorPath();
        path.AddOval(RectD.FromCenterAndSize(new VecD(5, 5), new VecD(10, 10)));
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(4, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(PathVerb.Conic, editablePath.SubShapes[0].Points[0].Verb.VerbType);
        
        Assert.Equal(new VecF(10, 5), editablePath.SubShapes[0].Points[0].Verb.From);
        Assert.Equal(new VecF(5, 10), editablePath.SubShapes[0].Points[0].Verb.To);
        Assert.InRange(editablePath.SubShapes[0].Points[0].Verb.ConicWeight, rangeLower, rangeUpper);
        
        Assert.Equal(PathVerb.Conic, editablePath.SubShapes[0].Points[1].Verb.VerbType);
        Assert.Equal(new VecF(5, 10), editablePath.SubShapes[0].Points[1].Verb.From);
        Assert.Equal(new VecF(0, 5), editablePath.SubShapes[0].Points[1].Verb.To);
        Assert.InRange(editablePath.SubShapes[0].Points[1].Verb.ConicWeight, rangeLower, rangeUpper);
        
        Assert.Equal(PathVerb.Conic, editablePath.SubShapes[0].Points[2].Verb.VerbType);
        Assert.Equal(new VecF(0, 5), editablePath.SubShapes[0].Points[2].Verb.From);
        Assert.Equal(new VecF(5, 0), editablePath.SubShapes[0].Points[2].Verb.To);
        Assert.InRange(editablePath.SubShapes[0].Points[2].Verb.ConicWeight, rangeLower, rangeUpper);
        
        Assert.Equal(PathVerb.Conic, editablePath.SubShapes[0].Points[3].Verb.VerbType);
        Assert.Equal(new VecF(5, 0), editablePath.SubShapes[0].Points[3].Verb.From);
        Assert.Equal(new VecF(10, 5), editablePath.SubShapes[0].Points[3].Verb.To);
        Assert.InRange(editablePath.SubShapes[0].Points[3].Verb.ConicWeight, rangeLower, rangeUpper);
    }

    [Fact]
    public void TestThatOverlappingPolyPointsReturnCorrectSubShapePoints()
    {
        VectorPath path = new VectorPath();
        
        /* 
         *     |\
         *     |_\
         *     |
         */
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(0, 2));
        path.LineTo(new VecF(0, 4));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(0, 2));
        path.Close();
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(5, editablePath.SubShapes[0].Points.Count);
        
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Verb.From);
        Assert.Equal(new VecF(0, 2), editablePath.SubShapes[0].Points[0].Verb.To);
        
        Assert.Equal(new VecF(0, 2), editablePath.SubShapes[0].Points[1].Verb.From);
        Assert.Equal(new VecF(0, 4), editablePath.SubShapes[0].Points[1].Verb.To);
        
        Assert.Equal(new VecF(0, 4), editablePath.SubShapes[0].Points[2].Verb.From);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[2].Verb.To);
        
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[3].Verb.From);
        Assert.Equal(new VecF(0, 2), editablePath.SubShapes[0].Points[3].Verb.To);
        
        Assert.Equal(new VecF(0, 2), editablePath.SubShapes[0].Points[4].Verb.From);
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[4].Verb.To);
    }

    [Fact]
    public void TestThatMultiSubShapesWithUnclosedReturnsCorrectPoints()
    {
        VectorPath path = new VectorPath();
        
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(0, 4));
        
        path.AddOval(RectD.FromCenterAndSize(new VecD(5, 5), new VecD(10, 10)));
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(2, editablePath.SubShapes.Count);
        
        Assert.Equal(3, editablePath.SubShapes[0].Points.Count);
        Assert.Equal(new VecF(0, 0), editablePath.SubShapes[0].Points[0].Position);
        Assert.Equal(new VecF(2, 2), editablePath.SubShapes[0].Points[1].Position);
        Assert.Equal(new VecF(0, 4), editablePath.SubShapes[0].Points[2].Position);
        
        Assert.False(editablePath.SubShapes[0].IsClosed);
        
        Assert.Equal(4, editablePath.SubShapes[1].Points.Count);
        Assert.Equal(new VecF(10, 5), editablePath.SubShapes[1].Points[0].Position);
        Assert.Equal(new VecF(5, 10), editablePath.SubShapes[1].Points[1].Position);
        Assert.Equal(new VecF(0, 5), editablePath.SubShapes[1].Points[2].Position);
        Assert.Equal(new VecF(5, 0), editablePath.SubShapes[1].Points[3].Position);
        
        Assert.True(editablePath.SubShapes[1].IsClosed);
    }
    
    [Fact]
    public void TestThatMoveToProducesEmptyVerb()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Single(editablePath.SubShapes);
        Assert.Single(editablePath.SubShapes[0].Points);
        
        Assert.Equal(null, editablePath.SubShapes[0].Points[0].Verb.VerbType);
    }
    
    [Fact]
    public void TestThatMultipleMoveToProduceEmptyVerbs()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.MoveTo(new VecF(2, 2));
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(2, editablePath.SubShapes.Count);
        
        Assert.Single(editablePath.SubShapes[0].Points);
        Assert.Single(editablePath.SubShapes[1].Points);
        
        Assert.Null(editablePath.SubShapes[0].Points[0].Verb.VerbType);
        Assert.Null(editablePath.SubShapes[1].Points[0].Verb.VerbType);
    }

    [Fact]
    public void TestThatEditingPointResultsInCorrectVectorPath()
    {
        VectorPath path = new VectorPath();
        path.MoveTo(new VecF(0, 0));
        path.LineTo(new VecF(2, 2));
        path.LineTo(new VecF(4, 4));
        path.Close();
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        editablePath.SubShapes[0].SetPointPosition(1, new VecF(3, 3), true);
        
        VectorPath newPath = editablePath.ToVectorPath();
        
        PathVerb[] sequence = [ PathVerb.Move, PathVerb.Line, PathVerb.Line, PathVerb.Line, PathVerb.Close, PathVerb.Done ];
        VecF[] points = [ new VecF(0, 0), new VecF(3, 3), new VecF(4, 4), new VecF(0, 0) ];

        int i = 0;
        foreach (var data in newPath)
        {
            Assert.Equal(sequence[i], data.verb);
            if(data.verb != PathVerb.Close && data.verb != PathVerb.Done)
            {
                Assert.Equal(points[i], Verb.GetPointFromVerb(data));
            }
            i++;
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 1)]
    [InlineData(7, 1)]
    public void TestThatGetSubShapeByPointIndexReturnsCorrectSubShapeIndex(int index, int expected)
    {
        VectorPath path = new VectorPath();
        path.AddOval(RectD.FromCenterAndSize(new VecD(5, 5), new VecD(10, 10)));
        path.AddOval(RectD.FromCenterAndSize(new VecD(15, 15), new VecD(20, 20)));
        
        EditableVectorPath editablePath = new EditableVectorPath(path);
        
        Assert.Equal(expected, editablePath.SubShapes.ToList().IndexOf(editablePath.GetSubShapeContainingIndex(index)));
    }
}