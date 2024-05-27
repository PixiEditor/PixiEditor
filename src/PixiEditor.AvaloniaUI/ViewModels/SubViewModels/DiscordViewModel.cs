using System.ComponentModel;
using DiscordRPC;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

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

    public DiscordViewModel(ViewModelMain owner, string clientId)
        : base(owner)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocumentChanged += DocumentChanged;
        this.clientId = clientId;

        Enabled = PixiEditorSettings.EnableRichPresence.Value;
        PixiEditorSettings.EnableRichPresence.ValueChanged += (_, value) => Enabled = value;
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

            richPresence.Details = PixiEditorSettings.ShowDocumentName.Value
                ? $"Editing {document.FileName.Limit(128)}" : "Editing an image";

            string state = string.Empty;

            if (PixiEditorSettings.ShowDocumentSize.Value)
            {
                state = $"{document.Width}x{document.Height}";
            }

            if (PixiEditorSettings.ShowDocumentSize.Value && PixiEditorSettings.ShowLayerCount.Value)
            {
                state += ", ";
            }

            if (PixiEditorSettings.ShowLayerCount.Value)
            {
                int count = CountLayers(document.StructureRoot);
                state += count == 1 ? "1 layer" : $"{count} layers";
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
            Details = "Staring at absolutely",
            State = "nothing",

            Assets = new Assets
            {
                LargeImageKey = "editorlogo",
                LargeImageText = "You've discovered PixiEditor's logo",
                SmallImageKey = "github",
                SmallImageText = "Download PixiEditor (pixieditor.net/download)!"
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
