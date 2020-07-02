using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using PixiEditor;

namespace PixiEditorTests
{
    [ExcludeFromCodeCoverage]
    public class ApplicationFixture
    {
        public ApplicationFixture()
        {
            if (Application.Current == null)
            {
                var app = new App();
                app.InitializeComponent();
            }
        }
    }
}
