using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.DataHolders.Guides;
using PixiEditor.Views.Dialogs.Guides;
using Direction = PixiEditor.Models.DataHolders.Guides.DirectionalGuide.Direction;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class GuidesViewModel : SubViewModel<ViewModelMain>
{
    private GuidesManager guideManager;

    public GuidesViewModel(ViewModelMain owner) : base(owner)
    { }

    [Command.Basic("PixiEditor.Guides.OpenManager", "OPEN_GUIDES_MANAGER", "OPEN_GUIDES_MANAGER_DESCRIPTIVE")]
    public void OpenGuideManager(Index openAt)
    {
        if (guideManager == null)
        {
            guideManager = new GuidesManager();
        }

        guideManager.Show();
        guideManager.Activate();
        guideManager.SelectGuide(openAt);
    }

    [Command.Basic("PixiEditor.Guides.AddLineGuide", "ADD_LINE_GUIDE", "ADD_LINE_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "Guides/LineGuide.png")]
    public void AddLineGuide()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument!;

        var position = document.SizeBindable / 2;
        var guide = new LineGuide(document)
        {
            X = position.X,
            Y = position.Y,
            Rotation = 45,
            Color = Colors.CadetBlue
        };

        document.Guides.Add(guide);
        OpenGuideManager(^0);
    }

    [Command.Basic("PixiEditor.Guides.AddVerticalGuide", Direction.Vertical, "ADD_VERTICAL_GUIDE", "ADD_VERTICAL_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "Guides/VerticalGuide.png")]
    [Command.Basic("PixiEditor.Guides.AddHorizontalGuide", Direction.Horizontal, "ADD_HORIZONTAL_GUIDE", "ADD_HORIZONTAL_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "Guides/HorizontalGuide.png")]
    public void AddDirectionalGuide(Direction direction)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument!;

        var documentSize = direction == Direction.Vertical ? document.SizeBindable.X : document.SizeBindable.Y;
        var guide = new DirectionalGuide(document, direction)
        {
            Offset = documentSize / 2,
            Color = Colors.CadetBlue,
        };

        document.Guides.Add(guide);
        OpenGuideManager(^0);
    }

    [Command.Basic("PixiEditor.Guides.AddRectangleGuide", "ADD_RECTANGLE_GUIDE", "ADD_RECTANGLE_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "Guides/RectangleGuide.png")]
    public void AddRectangleGuide()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument!;

        var margin = document.SizeBindable * 0.25;
        var guide = new RectangleGuide(document)
        {
            Left = margin.X,
            Top = margin.Y,
            Height = document.SizeBindable.X - margin.X * 2,
            Width = document.SizeBindable.Y - margin.Y * 2,
            Color = Colors.CadetBlue,
        };

        document.Guides.Add(guide);
        OpenGuideManager(^0);
    }

    [Command.Basic("PixiEditor.Guides.AddGridGuide", "ADD_GRID_GUIDE", "ADD_GRID_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "Guides/GridGuide.png")]
    public void AddGridGuide()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument!;

        var size = document.SizeBindable * 0.25;
        var guide = new GridGuide(document) 
        {
            VerticalOffset = size.X,
            HorizontalOffset = size.Y,
            VerticalColor = Colors.CadetBlue,
            HorizontalColor = Colors.CadetBlue
        };
        
        document.Guides.Add(guide);
        OpenGuideManager(^0);
    }

    [Command.Internal("PixiEditor.Guides.RemoveGuide", CanExecute = "PixiEditor.HasDocument")]
    public void RemoveGuide(Guide guide)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        document?.Guides.Remove(guide);
    }
}
