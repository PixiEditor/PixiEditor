using System;

namespace PixiEditor.SDK
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class PixiEditorExtensionAttribute : Attribute
    {
        public PixiEditorExtensionAttribute(Type extensionType)
        {
            ExtensionType = extensionType;
        }

        public Type ExtensionType { get; set; }
    }
}
