using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ViewModels.SubViewModels.Document;
internal class LineToolOverlayViewModel : NotifyableObject
{
    public event EventHandler<(VecD, VecD)> LineMoved;

    private VecD lineStart;
    public VecD LineStart
    {
        get => lineStart;
        set 
        {
            if (SetProperty(ref lineStart, value))
                LineMoved?.Invoke(this, (lineStart, lineEnd));
        }
    }

    private VecD lineEnd;
    public VecD LineEnd
    {
        get => lineEnd;
        set
        {
            if (SetProperty(ref lineEnd, value))
                LineMoved?.Invoke(this, (lineStart, lineEnd));
        }
    }

    private bool isEnabled;
    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }
}
