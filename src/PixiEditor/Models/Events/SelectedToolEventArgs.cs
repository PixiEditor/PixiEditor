using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Events
{
    public class SelectedToolEventArgs
    {
        public SelectedToolEventArgs(Tool oldTool, Tool newTool)
        {
            OldTool = oldTool;
            NewTool = newTool;
        }

        public Tool OldTool { get; set; }

        public Tool NewTool { get; set; }
    }
}
