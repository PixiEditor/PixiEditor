namespace PixiEditor.Views.Input;

internal class InputBoxEventArgs : EventArgs
{
    public string Input { get; set; }

    public InputBoxEventArgs(string input)
    {
        Input = input;
    }
}
