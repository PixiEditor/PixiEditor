using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Events;

public class InputBoxEventArgs : EventArgs
{
    public string Input { get; set; }

    public InputBoxEventArgs(string input)
    {
        Input = input;
    }
}