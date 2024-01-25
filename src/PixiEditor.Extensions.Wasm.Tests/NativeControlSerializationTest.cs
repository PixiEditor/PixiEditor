using System.Text;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Tests;

public class NativeControlSerializationTest
{
    [Fact]
    public void TestThatNoChildLayoutSerializesCorrectBytes()
    {
        NativeControl layout = new NativeControl("Layout");
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
        NativeControl layout = new NativeControl("Layout");
        layout.AddChild(new NativeControl("Center"));

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
        NativeControl layout = new NativeControl("Layout");
        NativeControl center = new NativeControl("Center");
        NativeControl text = new NativeControl("Text");
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

        NativeControl nativeControl = layout.Build();

        Assert.Equal("Layout", nativeControl.ControlId);
        Assert.Empty(nativeControl.Properties);
        Assert.Single(nativeControl.Children);

        Assert.Equal("Center", nativeControl.Children[0].ControlId);
        Assert.Empty(nativeControl.Children[0].Properties);

        Assert.Equal("Text", nativeControl.Children[0].Children[0].ControlId);
        Assert.Single(nativeControl.Children[0].Children[0].Properties);
        Assert.Equal("hello sexy.", nativeControl.Children[0].Children[0].Properties[0]);
    }
}
