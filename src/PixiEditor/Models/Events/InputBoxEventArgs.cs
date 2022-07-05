namespace PixiEditor.Models.Events;

internal class InputBoxEventArgs : EventArgs
{
    public string Input { get; set; }

    public InputBoxEventArgs(string input)
    {
        Input = input;
    }
}
