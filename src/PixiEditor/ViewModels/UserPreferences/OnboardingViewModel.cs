using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.UserPreferences.Settings;
using PixiEditor.Views.Shortcuts;

namespace PixiEditor.ViewModels.UserPreferences;

internal class OnboardingViewModel : PixiObservableObject
{
    private int page;
    private FormStep formStep;

    public int Page
    {
        get => page;
        set
        {
            value = Math.Clamp(value, 0, AllFormSteps.Count + 1);
            SetProperty(ref page, value);
        }
    }

    public FormStep FormStep
    {
        get => formStep;
        set
        {
            SetProperty(ref formStep, value);
            NextFormStepCommand.NotifyCanExecuteChanged();
            PreviousFormStepCommand.NotifyCanExecuteChanged();

            foreach (var step in AllFormSteps)
            {
                step.IsActive = step.Step == value.Step;
            }
        }
    }

    public ObservableCollection<FormStep> AllFormSteps { get; } = new()
    {
        new FormStep { Title = new LocalizedString("ONB_SELECT_PRIMARY_TOOLSET"), Step = 0 },
        new FormStep { Title = new LocalizedString("LANGUAGE"), Step = 1 },
        new FormStep { Title = new LocalizedString("ONB_SHORTCUTS"), Step = 2 },
        new FormStep { Title = new LocalizedString("ONB_ANALYTICS"), Step = 3 },
#if !FOUNDERS_PACK_REQUIRED
        new FormStep { Title = new LocalizedString("FOUNDERS_BUNDLE"), Step = 4 }
#endif
    };

    public RelayCommand NextFormStepCommand { get; }
    public RelayCommand PreviousFormStepCommand { get; }

    public GeneralSettings GeneralSettings { get; } = new();
    public FileSettings FileSettings { get; } = new();
    public ToolsSettings ToolSettings { get; } = new();

    public ObservableCollection<SelectionCard<ShortcutProvider>> Templates { get; set; }
    public ObservableCollection<SelectionCard<IToolSetHandler>> ToolSets { get; }

    public AsyncRelayCommand<ShortcutProvider> SelectShortcutCommand { get; }

    public RelayCommand<IToolSetHandler> SelectToolsetCommand { get; }

    public string FoundersBundleLink => UserViewModel.FoundersBundleLink;

    Dictionary<string, VecI> DefaultNewFileSizes = new()
    {
        { "PIXEL_ART_TOOLSET", new VecI(64, 64) },
        { "PAINT_TOOLSET", new VecI(1920, 1080) },
        { "VECTOR_TOOLSET", new VecI(512, 512) }
    };

    public OnboardingViewModel()
    {
        NextFormStepCommand = new RelayCommand(NextFormStep, CanNextFormStep);
        PreviousFormStepCommand = new RelayCommand(PreviousFormStep, CanPreviousFormStep);

        SelectToolsetCommand = new RelayCommand<IToolSetHandler>(x =>
        {
            foreach (var toolset in ToolSets)
            {
                toolset.IsSelected = toolset.Item == x;
            }

            ToolSettings.PrimaryToolset = x.Name;
            if (DefaultNewFileSizes.ContainsKey(x.Name))
            {
                FileSettings.DefaultNewFileWidth = DefaultNewFileSizes[x.Name].X;
                FileSettings.DefaultNewFileHeight = DefaultNewFileSizes[x.Name].Y;
            }
        });

        SelectShortcutCommand = new AsyncRelayCommand<ShortcutProvider>(async x =>
        {
            foreach (var template in Templates)
            {
                template.IsSelected = template.Item == x;
            }

            if (x == Templates[0].Item)
            {
                CommandController.Current.ResetShortcuts();
            }
            else
            {
                await ImportShortcutTemplatePopup.ImportFromProvider(x, true);
            }
        });

        FormStep = AllFormSteps[0];
        GeneralSettings = new GeneralSettings();
        Templates = new ObservableCollection<SelectionCard<ShortcutProvider>>(ShortcutProvider.GetProviders()
            .Select(x => new SelectionCard<ShortcutProvider>(x, SelectShortcutCommand)));
        Templates.Insert(0,
            new SelectionCard<ShortcutProvider>(
                new ShortcutProvider("PixiEditor")
                {
                    LogoPath = "/Images/PixiEditorLogo.svg", HoverLogoPath = "/Images/PixiEditorLogo.svg"
                },
                SelectShortcutCommand) { IsSelected = true });
        ToolSets = new ObservableCollection<SelectionCard<IToolSetHandler>>(
            ViewModelMain.Current.ToolsSubViewModel.AllToolSets.Select(x =>
                new SelectionCard<IToolSetHandler>(x, SelectToolsetCommand)));

        var firstToolSet = ToolSets.FirstOrDefault();
        if (firstToolSet != null)
        {
            firstToolSet.IsSelected = true;
        }
    }

    public void NextFormStep()
    {
        if (FormStep.Step == AllFormSteps.Count - 1)
        {
            NextPage();
            return;
        }

        FormStep = AllFormSteps[FormStep.Step + 1];
    }

    public void PreviousFormStep()
    {
        if (FormStep.Step == 0)
        {
            PreviousPage();
            return;
        }

        FormStep = AllFormSteps[FormStep.Step - 1];
    }

    public bool CanNextFormStep()
    {
        return FormStep.Step < AllFormSteps.Count;
    }

    public bool CanPreviousFormStep()
    {
        return true;
    }

    public void NextPage()
    {
        Page++;
    }

    public void PreviousPage()
    {
        Page--;
    }

    public void OnFinish()
    {
        var userViewModel = ViewModelMain.Current.UserViewModel;
        if (userViewModel.User is { IsLoggedIn: true })
        {
            foreach (var product in userViewModel.OwnedProducts)
            {
                if (!product.IsInstalled)
                {
                    Dispatcher.UIThread.InvokeAsync(async () => await product.InstallCommand.ExecuteAsync(null));
                }
            }

        }
        ViewModelMain.Current.WindowSubViewModel.OpenHelloThereWindow();
    }
}

public class FormStep : ObservableObject
{
    public LocalizedString Title { get; set; }
    public int Step { get; set; }
    private bool isActive;

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }
}

public class SelectionCard<T> : ObservableObject
{
    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            SetProperty(ref isSelected, value);
        }
    }

    public ICommand SelectCommand { get; }

    public T Item { get; set; }

    public SelectionCard(T item, ICommand selectCommand)
    {
        Item = item;
        SelectCommand = selectCommand;
    }
}
