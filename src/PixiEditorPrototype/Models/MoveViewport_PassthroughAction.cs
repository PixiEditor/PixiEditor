using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditorPrototype.Models
{
    internal record class MoveViewport_PassthroughAction : IAction, IChangeInfo
    {
        public MoveViewport_PassthroughAction(Vector2d center, Vector2d size, double angle, Vector2d realSize)
        {
            Center = center;
            Size = size;
            Angle = angle;
            RealSize = realSize;
        }

        public Vector2d Center { get; }
        public Vector2d Size { get; }
        public Vector2d RealSize { get; }
        public double Angle { get; }
    }
}
