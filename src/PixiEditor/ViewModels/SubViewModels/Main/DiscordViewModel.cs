using System.ComponentModel;
using DiscordRPC;
using PixiEditor.Localization;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class DiscordViewModel : SubViewModel<ViewModelMain>, IDisposable
{
    private DiscordRpcClient client;
    private string clientId;
    private DocumentViewModel currentDocument;

    public bool Enabled
    {
        get => client != null;
        set
        {
            if (Enabled != value)
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
    }

    private bool showDocumentName = IPreferences.Current.GetPreference(nameof(ShowDocumentName), false);

    public bool ShowDocumentName
    {
        get => showDocumentName;
        set
        {
            if (showDocumentName != value)
            {
                showDocumentName = value;
                UpdatePresence(currentDocument);
            }
        }
    }

    private bool showDocumentSize = IPreferences.Current.GetPreference(nameof(ShowDocumentSize), true);

    public bool ShowDocumentSize
    {
        get => showDocumentSize;
        set
        {
            if (showDocumentSize != value)
            {
                showDocumentSize = value;
                UpdatePresence(currentDocument);
            }
        }
    }

    private bool showLayerCount = IPreferences.Current.GetPreference(nameof(ShowLayerCount), true);

    public bool ShowLayerCount
    {
        get => showLayerCount;
        set
        {
            if (showLayerCount != value)
            {
                showLayerCount = value;
                UpdatePresence(currentDocument);
            }
        }
    }

    public DiscordViewModel(ViewModelMain owner, string clientId)
        : base(owner)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocumentChanged += DocumentChanged;
        this.clientId = clientId;

        Enabled = IPreferences.Current.GetPreference("EnableRichPresence", true);
        IPreferences.Current.AddCallback("EnableRichPresence", x => Enabled = (bool)x);
        IPreferences.Current.AddCallback(nameof(ShowDocumentName), x => ShowDocumentName = (bool)x);
        IPreferences.Current.AddCallback(nameof(ShowDocumentSize), x => ShowDocumentSize = (bool)x);
        IPreferences.Current.AddCallback(nameof(ShowLayerCount), x => ShowLayerCount = (bool)x);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Enabled = false;
    }

    public void Start()
    {
        client = new DiscordRpcClient(clientId);
        client.OnReady += OnReady;
        client.Initialize();
    }

    public void Stop()
    {
        client.ClearPresence();
        client.Dispose();
        client = null;
    }

    public void UpdatePresence(DocumentViewModel? document)
    {
        if (client == null)
        {
            return;
        }

        RichPresence richPresence = NewDefaultRP();

        if (document != null)
        {
            richPresence.WithTimestamps(new Timestamps(document.OpenedUTC));

            richPresence.Details = ShowDocumentName
                ? new LocalizedString("EDITING_IMG_DETAIL", document.FileName.Limit(128))
                : new LocalizedString("EDITING_IMG");

            string state = string.Empty;

            if (ShowDocumentSize)
            {
                state = $"{document.Width}x{document.Height}";
            }

            if (ShowDocumentSize && ShowLayerCount)
            {
                state += ", ";
            }

            if (ShowLayerCount)
            {
                int count = CountLayers(document.StructureRoot);
                state += count == 1 ? new LocalizedString("ONE_LAYER") : new LocalizedString("LAYERS", count);
            }

            richPresence.State = state;
        }

        client.SetPresence(richPresence);
    }

    private int CountLayers(FolderViewModel folder)
    {
        int counter = 0;
        foreach (var child in folder.Children)
        {
            if (child is LayerViewModel)
                counter++;
            else if (child is FolderViewModel innerFolder)
                counter += CountLayers(innerFolder);
        }
        return counter;
    }

    public void Dispose()
    {
        Enabled = false;
        GC.SuppressFinalize(this);
    }

    private static RichPresence NewDefaultRP()
    {
        return new RichPresence
        {
            Details = new LocalizedString("DISCORD_DETAILS"),
            State = new LocalizedString("DISCORD_STATE"),

            Assets = new Assets
            {
                LargeImageKey = "editorlogo",
                LargeImageText = new LocalizedString("DISCORD_LARGE_IMAGE"),
                SmallImageKey = "github",
                SmallImageText = new LocalizedString("DISCORD_SMALL_IMAGE")
            },
            Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            }
        };
    }
    
    private void DocumentChanged(object sender, DocumentChangedEventArgs e)
    {
        if (currentDocument != null)
        {
            currentDocument.PropertyChanged -= DocumentPropertyChanged;
            currentDocument.LayersChanged -= DocumentLayerChanged;
        }

        currentDocument = e.NewDocument;

        if (currentDocument != null)
        {
            UpdatePresence(currentDocument);
            currentDocument.PropertyChanged += DocumentPropertyChanged;
            currentDocument.LayersChanged += DocumentLayerChanged;
        }
    }

    private void DocumentLayerChanged(object sender, LayersChangedEventArgs e)
    {
        UpdatePresence(currentDocument);
    }

    private void DocumentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(currentDocument.FileName)
            || e.PropertyName == nameof(currentDocument.Width)
            || e.PropertyName == nameof(currentDocument.Height))
        {
            UpdatePresence(currentDocument);
        }
    }

    private void OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
    {
        UpdatePresence(Owner.DocumentManagerSubViewModel.ActiveDocument);
    }

    ~DiscordViewModel()
    {
        Enabled = false;
    }
}
