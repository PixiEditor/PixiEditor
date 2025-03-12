using System.Reflection;
using System.Xml;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Utils;

namespace PixiEditor.Tests;

public class SvgTests
{
    [Fact]
    public void TestThatEmptySvgIsParsedCorrectly()
    {
        string svg = "<svg></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        Assert.Empty(document.Children);
    }

    [Theory]
    [InlineData("<svg viewBox=\"0 0 100 100\"></svg>", 0, 0, 100, 100)]
    [InlineData("<svg width=\"100\" height=\"100\"></svg>", 0, 0, 100, 100)]
    [InlineData("<svg x=\"0\" y=\"0\" width=\"100\" height=\"100\"></svg>", 0, 0, 100, 100)]
    [InlineData("<svg viewBox=\"0 0 100 100\" width=\"50\" height=\"50\"></svg>", 0, 0, 50, 50)]
    [InlineData("<svg viewBox=\"-50 -50 128 128\" width=\"100\" x=\"1\"></svg>", 1, -50, 100, 128)]
    public void TestThatSvgBoundsAreParsedCorrectly(string svg, double x, double y, double width, double height)
    {
        SvgDocument document = SvgDocument.Parse(svg);
        Assert.Equal(x, document.ViewBox.Unit.Value.Value.X);
        Assert.Equal(y, document.ViewBox.Unit.Value.Value.Y);
        Assert.Equal(width, document.ViewBox.Unit.Value.Value.Width);
        Assert.Equal(height, document.ViewBox.Unit.Value.Value.Height);
    }

    [Theory]
    [InlineData("<svg><rect/></svg>", 1)]
    [InlineData("<svg><rect/><circle/></svg>", 2)]
    [InlineData("<svg><rect/><circle/><ellipse/></svg>", 3)]
    public void TestThatSvgElementsCountIsParsedCorrectly(string svg, int elements)
    {
        SvgDocument document = SvgDocument.Parse(svg);
        Assert.Equal(elements, document.Children.Count);
    }

    [Theory]
    [InlineData("<svg><rect/></svg>", "rect")]
    [InlineData("<svg><circle/></svg>", "circle")]
    [InlineData("<svg><ellipse/></svg>", "ellipse")]
    [InlineData("<svg><someArbitraryElement/></svg>", null)]
    public void TestThatSvgElementsAreParsedCorrectly(string svg, string? element)
    {
        SvgDocument document = SvgDocument.Parse(svg);
        if (element == null)
        {
            Assert.Empty(document.Children);
            return;
        }

        Assert.Equal(element, document.Children[0].TagName);
    }

    [Theory]
    [InlineData("<svg><rect/></svg>", typeof(SvgRectangle))]
    [InlineData("<svg><circle/></svg>", typeof(SvgCircle))]
    [InlineData("<svg><ellipse/></svg>", typeof(SvgEllipse))]
    [InlineData("<svg><g/></svg>", typeof(SvgGroup))]
    [InlineData("<svg><line/></svg>", typeof(SvgLine))]
    [InlineData("<svg><path/></svg>", typeof(SvgPath))]
    [InlineData("<svg><mask/></svg>", typeof(SvgMask))]
    [InlineData("<svg><image/></svg>", typeof(SvgImage))]
    [InlineData("<svg><someArbitraryElement/></svg>", null)]
    public void TestThatSvgElementsAreParsedToCorrectType(string svg, Type? elementType)
    {
        SvgDocument document = SvgDocument.Parse(svg);
        if (elementType == null)
        {
            Assert.Empty(document.Children);
            return;
        }

        Assert.IsType(elementType, document.Children[0]);
    }

    [Fact]
    public void TestThatRectIsParsedCorrectly()
    {
        string svg = "<svg><rect x=\"10\" y=\"20\" width=\"30\" height=\"40\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgRectangle rect = (SvgRectangle)document.Children[0];

        Assert.NotNull(rect);
        Assert.NotNull(rect.X.Unit);
        Assert.NotNull(rect.Y.Unit);
        Assert.NotNull(rect.Width.Unit);
        Assert.NotNull(rect.Height.Unit);

        Assert.Equal(10, rect.X.Unit.Value.Value);
        Assert.Equal(20, rect.Y.Unit.Value.Value);
        Assert.Equal(30, rect.Width.Unit.Value.Value);
        Assert.Equal(40, rect.Height.Unit.Value.Value);
    }

    [Fact]
    public void TestThatCircleIsParsedCorrectly()
    {
        string svg = "<svg><circle cx=\"10\" cy=\"20\" r=\"30\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgCircle circle = (SvgCircle)document.Children[0];

        Assert.NotNull(circle);
        Assert.NotNull(circle.Cx.Unit);
        Assert.NotNull(circle.Cy.Unit);
        Assert.NotNull(circle.R.Unit);

        Assert.Equal(10, circle.Cx.Unit.Value.Value);
        Assert.Equal(20, circle.Cy.Unit.Value.Value);
        Assert.Equal(30, circle.R.Unit.Value.Value);
    }

    [Fact]
    public void TestThatEllipseIsParsedCorrectly()
    {
        string svg = "<svg><ellipse cx=\"10\" cy=\"20\" rx=\"30\" ry=\"40\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgEllipse ellipse = (SvgEllipse)document.Children[0];

        Assert.NotNull(ellipse);
        Assert.NotNull(ellipse.Cx.Unit);
        Assert.NotNull(ellipse.Cy.Unit);
        Assert.NotNull(ellipse.Rx.Unit);
        Assert.NotNull(ellipse.Ry.Unit);

        Assert.Equal(10, ellipse.Cx.Unit.Value.Value);
        Assert.Equal(20, ellipse.Cy.Unit.Value.Value);
        Assert.Equal(30, ellipse.Rx.Unit.Value.Value);
        Assert.Equal(40, ellipse.Ry.Unit.Value.Value);
    }

    [Fact]
    public void TestThatGroupIsParsedCorrectly()
    {
        string svg = "<svg><g><rect/></g></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgGroup group = (SvgGroup)document.Children[0];

        Assert.NotNull(group);
        Assert.Single(group.Children);
        Assert.IsType<SvgRectangle>(group.Children[0]);
    }

    [Fact]
    public void TestThatLineIsParsedCorrectly()
    {
        string svg = "<svg><line x1=\"10\" y1=\"20\" x2=\"30\" y2=\"40\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgLine line = (SvgLine)document.Children[0];

        Assert.NotNull(line);
        Assert.NotNull(line.X1.Unit);
        Assert.NotNull(line.Y1.Unit);
        Assert.NotNull(line.X2.Unit);
        Assert.NotNull(line.Y2.Unit);

        Assert.Equal(10, line.X1.Unit.Value.Value);
        Assert.Equal(20, line.Y1.Unit.Value.Value);
        Assert.Equal(30, line.X2.Unit.Value.Value);
        Assert.Equal(40, line.Y2.Unit.Value.Value);
    }

    [Fact]
    public void TestThatPathIsParsedCorrectly()
    {
        string svg = "<svg><path d=\"M10 20 L30 40\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgPath path = (SvgPath)document.Children[0];

        Assert.NotNull(path);
        Assert.NotNull(path.PathData.Unit);

        Assert.Equal("M10 20 L30 40", path.PathData.Unit.Value.Value);
    }

    [Fact]
    public void TestThatMaskIsParsedCorrectly()
    {
        string svg = "<svg><mask><rect/></mask></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgMask mask = (SvgMask)document.Children[0];

        Assert.NotNull(mask);
        Assert.Single(mask.Children);
        Assert.IsType<SvgRectangle>(mask.Children[0]);
    }

    [Fact]
    public void TestThatImageIsParsedCorrectly()
    {
        string svg = "<svg><image x=\"10\" y=\"20\" width=\"30\" height=\"40\"/></svg>";
        SvgDocument document = SvgDocument.Parse(svg);
        SvgImage image = (SvgImage)document.Children[0];

        Assert.NotNull(image);
        Assert.NotNull(image.X.Unit);
        Assert.NotNull(image.Y.Unit);
        Assert.NotNull(image.Width.Unit);
        Assert.NotNull(image.Height.Unit);

        Assert.Equal(10, image.X.Unit.Value.Value);
        Assert.Equal(20, image.Y.Unit.Value.Value);
        Assert.Equal(30, image.Width.Unit.Value.Value);
        Assert.Equal(40, image.Height.Unit.Value.Value);
    }

    [Fact]
    public void TestThatAllAssemblySvgElementsParseData()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SvgElement))!;
        Type[] types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(SvgElement)) && !t.IsAbstract).ToArray();

        foreach (Type type in types)
        {
            SvgElement element = (SvgElement)Activator.CreateInstance(type)!;
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            writer.Write($"<svg><{element.TagName}/></svg>");
            using XmlReader reader = XmlReader.Create(stream);
            
            element.ParseData(reader, new SvgDefs());
        }
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#ff0000")]
    [InlineData("rgb(255, 0, 0)")]
    [InlineData("hsl(0, 100%, 50%)")]
    [InlineData("hsla(0, 100%, 50%, 255)")]
    [InlineData("rgba(255, 0, 0, 255)")]
    public void TestThatDifferentColorFormatsGetsParsedToTheSameRedValue(string colorInput)
    {
        if(SvgColorUtility.TryConvertStringToColor(colorInput, out Color color))
        {
            Assert.Equal(255, color.R);
            Assert.Equal(0, color.G);
            Assert.Equal(0, color.B);
            Assert.Equal(255, color.A);
        }
        else
        {
            Assert.Fail();
        }
    }
}