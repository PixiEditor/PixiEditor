using System.Text;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.Extensions.Sdk.Tests;

public class NativeControlSerializationTest
{
    [Fact]
    public void TestThatNoChildLayoutSerializesCorrectBytes()
    {
        ControlDefinition layout = new ControlDefinition(0, typeof(Layout));
        layout.AddProperty("Title");

        int uniqueId = 0;
        byte[] uniqueIdBytes = BitConverter.GetBytes(uniqueId);

        string controlId = "Layout";
        byte[] controlIdBytes = Encoding.UTF8.GetBytes(controlId);

        int propertiesCount = 1;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int stringLen = "Title".Length;
        byte[] stringLenBytes = BitConverter.GetBytes(stringLen);

        byte[] titleBytes = Encoding.UTF8.GetBytes("Title");

        int childCount = 0;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        List<byte> expectedBytes = new();
        expectedBytes.AddRange(uniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(controlId.Length));
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
        ControlDefinition layout = new ControlDefinition(0, typeof(Layout));
        layout.AddChild(new ControlDefinition(1, typeof(Center)));

        int uniqueId = 0;
        byte[] uniqueIdBytes = BitConverter.GetBytes(uniqueId);

        string controlId = "Layout";
        byte[] controlIdBytes = Encoding.UTF8.GetBytes(controlId);

        int propertiesCount = 0;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int childCount = 1;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        int childUniqueId = 1;
        byte[] childUniqueIdBytes = BitConverter.GetBytes(childUniqueId);

        string childControlId = "Center";
        byte[] childControlIdBytes = Encoding.UTF8.GetBytes(childControlId);

        int childPropertiesCount = 0;
        byte[] childPropertiesCountBytes = BitConverter.GetBytes(childPropertiesCount);

        int childChildCount = 0;
        byte[] childChildCountBytes = BitConverter.GetBytes(childChildCount);

        List<byte> expectedBytes = new();
        expectedBytes.AddRange(uniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(controlId.Length));
        expectedBytes.AddRange(controlIdBytes);
        expectedBytes.AddRange(propertiesCountBytes);
        expectedBytes.AddRange(childCountBytes);
        expectedBytes.AddRange(childUniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(childControlId.Length));
        expectedBytes.AddRange(childControlIdBytes);
        expectedBytes.AddRange(childPropertiesCountBytes);
        expectedBytes.AddRange(childChildCountBytes);

        Assert.Equal(expectedBytes.ToArray(), layout.Serialize().ToArray());
    }

    [Fact]
    public void TestThatBuildNativeBuildsPropertyBytesCorrectly()
    {
        Layout layout = new Layout();
        var definition = layout.BuildNative();

        Assert.Single(definition.Properties); // Cursor

        byte[] serialized = definition.SerializeBytes();

        Assert.Equal(23, serialized.Length);
    }

    [Fact]
    public void TestThatStatelessElementSerializesBytesProperly()
    {
        WindowContentElement layout = new WindowContentElement();

        var definition = layout.BuildNative();
        var serialized = definition.SerializeBytes();

        Assert.Equal(23, serialized.Length);
    }

    [Fact]
    public void TestThatChildNestedLayoutSerializesCorrectBytes()
    {
        ControlDefinition layout = new ControlDefinition(0, typeof(Layout));
        ControlDefinition center = new ControlDefinition(1, typeof(Center));
        ControlDefinition text = new ControlDefinition(2, typeof(Text));
        text.AddProperty("Hello world");
        center.AddChild(text);
        layout.AddChild(center);

        int uniqueId = 0;
        byte[] uniqueIdBytes = BitConverter.GetBytes(uniqueId);

        string controlId = "Layout";
        byte[] controlIdBytes = Encoding.UTF8.GetBytes(controlId);

        int propertiesCount = 0;
        byte[] propertiesCountBytes = BitConverter.GetBytes(propertiesCount);

        int childCount = 1;
        byte[] childCountBytes = BitConverter.GetBytes(childCount);

        int childUniqueId = 1;
        byte[] childUniqueIdBytes = BitConverter.GetBytes(childUniqueId);

        string childControlId = "Center";
        byte[] childControlIdBytes = Encoding.UTF8.GetBytes(childControlId);

        int childPropertiesCount = 0;
        byte[] childPropertiesCountBytes = BitConverter.GetBytes(childPropertiesCount);

        int childChildCount = 1;
        byte[] childChildCountBytes = BitConverter.GetBytes(childChildCount);

        int textUniqueId = 2;
        byte[] textUniqueIdBytes = BitConverter.GetBytes(textUniqueId);

        string textControlId = "Text";
        byte[] textControlIdBytes = Encoding.UTF8.GetBytes(textControlId);

        int textPropertiesCount = 1;
        byte[] textPropertiesCountBytes = BitConverter.GetBytes(textPropertiesCount);

        int textStringLen = "Hello world".Length;
        byte[] textStringLenBytes = BitConverter.GetBytes(textStringLen);

        byte[] textTitleBytes = Encoding.UTF8.GetBytes("Hello world");

        int textChildCount = 0;
        byte[] textChildCountBytes = BitConverter.GetBytes(textChildCount);


        List<byte> expectedBytes = new();
        expectedBytes.AddRange(uniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(controlId.Length));
        expectedBytes.AddRange(controlIdBytes);
        expectedBytes.AddRange(propertiesCountBytes);
        expectedBytes.AddRange(childCountBytes);

        expectedBytes.AddRange(childUniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(childControlId.Length));
        expectedBytes.AddRange(childControlIdBytes);
        expectedBytes.AddRange(childPropertiesCountBytes);
        expectedBytes.AddRange(childChildCountBytes);

        expectedBytes.AddRange(textUniqueIdBytes);
        expectedBytes.AddRange(BitConverter.GetBytes(textControlId.Length));
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

        ControlDefinition compiledControl = layout.BuildNative();

        Assert.Equal("Layout", compiledControl.ControlTypeId);
        Assert.Single(compiledControl.Properties);
        Assert.Single(compiledControl.Children);

        Assert.Equal("Center", compiledControl.Children[0].ControlTypeId);
        Assert.Single(compiledControl.Children[0].Properties);

        Assert.Equal("Text", compiledControl.Children[0].Children[0].ControlTypeId);
        Assert.True(compiledControl.Children[0].Children[0].Properties.Count > 0);
        Assert.Equal("hello sexy.", compiledControl.Children[0].Children[0].Properties[1].value);
    }

    [Fact]
    public void TestThatBuildButtonQueuesEvents()
    {
        Button button = new Button(
            child: new Text("hello sexy."),
            onClick: _ => { });

        button.BuildNative();

        Assert.Contains(button.BuildQueuedEvents, x => x == "Click");
    }
}