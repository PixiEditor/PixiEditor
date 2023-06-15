using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.DataHolders.Guides;
using PixiEditor.Views.Dialogs.Guides;

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
    }

    [Command.Basic("PixiEditor.Guides.AddLineGuide", "ADD_LINE_GUIDE", "ADD_LINE_GUIDE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument")]
    public void AddLineGuide()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;

        var guide = new LineGuide(document)
        {
            Position = document.SizeBindable / 2,
            Rotation = Math.PI / 4
        };

        document.Guides.Add(guide);
        OpenGuideManager(^0);
    }
}
