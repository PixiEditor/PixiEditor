namespace PixiEditor.Extensions.Sdk;

public abstract class PixiEditorExtension
{
    public static PixiEditorApi Api { get; } = new PixiEditorApi();
    
    /// <summary>
    ///     Called when extension is loaded. Api is not initialized at this point. All extensions are loaded before initialization.
    /// </summary>
    public virtual void OnLoaded() { }
    
    /// <summary>
    ///     Called when extension is initialized. Api is initialized and ready to use.
    /// </summary>
    public virtual void OnInitialized() { }
}
