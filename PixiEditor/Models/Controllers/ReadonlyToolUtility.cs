using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Controllers
{
    public class ReadonlyToolUtility
    {
        public BitmapManager Manager { get; set; }

        public ReadonlyToolUtility(BitmapManager manager)
        {
            Manager = manager;
        }

        public void ExecuteTool(ReadonlyTool tool)
        {            
            tool.Use();
        }

    }
}
