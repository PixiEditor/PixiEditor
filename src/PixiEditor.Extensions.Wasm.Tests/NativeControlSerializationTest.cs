using System.Text;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Tests;

public class NativeControlSerializationTest
{
    [Fact]
    public void TestThatNoChildLayoutSerializesCorrectBytes()
    {
        CompiledControl layout = new CompiledControl(0, "Layout");
        layout.AddProperty("Title");

        int controlId = ByteMap.ControlMap["Layout"];
        byte[] controlIdBytes = BitConverter.GetBytes(controlId);

        int propertiesCount = 1;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int stringLen = "Title".Length;
        byte[] stringLenBytes = BitConverter.GetBytes(stringLen);

        byte[] titleBytes = Encoding.UTF8.GetBytes("Title");

        int childCount = 0;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        List<byte> expectedBytes = new();
        expectedBytes.AddRange(controlIdBytes);
        expectedBytes.AddRange(propertiesCountBytes);
        expectedBytes.Add(ByteMap.GetTypeByteId(typeof(string)));
        expectedBytes.AddRange(stringLenBytes);
        expectedBytes.AddRange(titleBytes);
        expectedBytes.AddRange(childCountBytes);

        Assert.Equal(expectedBytes.ToArray(), layout.Serialize().ToArray());
    }

    [Fact]
    public void TestThatChildLayoutSerializesCorrectBytes()
    {
        CompiledControl layout = new CompiledControl(0, "Layout");
        layout.AddChild(new CompiledControl(0, "Center"));

        int controlId = ByteMap.ControlMap["Layout"];
        byte[] controlIdBytes = BitConverter.GetBytes(controlId);

        int propertiesCount = 0;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int childCount = 1;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        int childControlId = ByteMap.ControlMap["Center"];
        byte[] childControlIdBytes = BitConverter.GetBytes(childControlId);

        int childPropertiesCount = 0;
        byte[] childPropertiesCountBytes = BitConverter.GetBytes(childPropertiesCount);

        int childChildCount = 0;
        byte[] childChildCountBytes = BitConverter.GetBytes(childChildCount);

        List<byte> expectedBytes = new();
        expectedBytes.AddRange(controlIdBytes);
        expectedBytes.AddRange(propertiesCountBytes);
        expectedBytes.AddRange(childCountBytes);
        expectedBytes.AddRange(childControlIdBytes);
        expectedBytes.AddRange(childPropertiesCountBytes);
        expectedBytes.AddRange(childChildCountBytes);

        Assert.Equal(expectedBytes.ToArray(), layout.Serialize().ToArray());
    }

    [Fact]
    public void TestThatChildNestedLayoutSerializesCorrectBytes()
    {
        CompiledControl layout = new CompiledControl(0, "Layout");
        CompiledControl center = new CompiledControl(1, "Center");
        CompiledControl text = new CompiledControl(2, "Text");
        text.AddProperty("Hello world");
        center.AddChild(text);
        layout.AddChild(center);

        int controlId = ByteMap.ControlMap["Layout"];
        byte[] controlIdBytes = BitConverter.GetBytes(controlId);

        int propertiesCount = 0;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int childCount = 1;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        int childControlId = ByteMap.ControlMap["Center"];
        byte[] childControlIdBytes = BitConverter.GetBytes(childControlId);

        int childPropertiesCount = 0;
        byte[] childPropertiesCountBytes = BitConverter.GetBytes(childPropertiesCount);

        int childChildCount = 1;
        byte[] childChildCountBytes = BitConverter.GetBytes(childChildCount);

        int textControlId = ByteMap.ControlMap["Text"];
        byte[] textControlIdBytes = BitConverter.GetBytes(textControlId);

        int textPropertiesCount = 1;
        byte[] textPropertiesCountBytes = BitConverter.GetBytes(textPropertiesCount);

        int textStringLen = "Hello world".Length;
        byte[] textStringLenBytes = BitConverter.GetBytes(textStringLen);

        byte[] textTitleBytes = Encoding.UTF8.GetBytes("Hello world");

        int textChildCount = 0;
        byte[] textChildCountBytes = BitConverter.GetBytes(textChildCount);


        List<byte> expectedBytes = new();
        expectedBytes.AddRange(controlIdBytes);
        expectedBytes.AddRange(propertiesCountBytes);
        expectedBytes.AddRange(childCountBytes);

        expectedBytes.AddRange(childControlIdBytes);
        expectedBytes.AddRange(childPropertiesCountBytes);
        expectedBytes.AddRange(childChildCountBytes);

        expectedBytes.AddRange(textControlIdBytes);
        expectedBytes.AddRange(textPropertiesCountBytes);
        expectedBytes.Add(ByteMap.GetTypeByteId(typeof(string)));
        expectedBytes.AddRange(textStringLenBytes);
        expectedBytes.AddRange(textTitleBytes);
        expectedBytes.AddRange(textChildCountBytes);

        Assert.Equal(expectedBytes.ToArray(), layout.Serialize().ToArray());
    }

    [Fact]
    public void TestThatLayoutBuilderProperlyConvertsToNativeControls()
    {
        Layout layout = new Layout(
            new Center(
                child: new Text("hello sexy.")));

        CompiledControl compiledControl = layout.Build();

        Assert.Equal("Layout", compiledControl.ControlTypeId);
        Assert.Empty(compiledControl.Properties);
        Assert.Single(compiledControl.Children);

        Assert.Equal("Center", compiledControl.Children[0].ControlTypeId);
        Assert.Empty(compiledControl.Children[0].Properties);

        Assert.Equal("Text", compiledControl.Children[0].Children[0].ControlTypeId);
        Assert.Single(compiledControl.Children[0].Children[0].Properties);
        Assert.Equal("hello sexy.", compiledControl.Children[0].Children[0].Properties[0]);
    }

    [Fact]
    public void TestThatBuildButtonQueuesEvents()
    {
        Button button = new Button(
            child: new Text("hello sexy."),
            onClick: _ => { });

        button.Build();

        Assert.Contains(button.BuildQueuedEvents, x => x == "Click");
    }
}
