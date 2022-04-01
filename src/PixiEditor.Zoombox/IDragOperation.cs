using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal interface IDragOperation
    {
        void Start(MouseButtonEventArgs e);

        void Update(MouseEventArgs e);

        void Terminate();
    }
}
