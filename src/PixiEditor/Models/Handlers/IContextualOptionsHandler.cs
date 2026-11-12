using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.DocumentModels;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.Handlers;

internal interface IContextualOptionsHandler : IHandler
{
    public VecD Position { get; set; }
    public bool IsVisible { get; set; }
    public void SetOptions(IEnumerable<ContextualOption> options);
    public void Show(VecD position);
    public void Hide();
}
