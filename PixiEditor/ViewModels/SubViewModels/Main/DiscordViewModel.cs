using DiscordRPC;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.UserPreferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DiscordViewModel : SubViewModel<ViewModelMain>
    {
        private DiscordRpcClient client;
        private string clientId;
        private Document currentDocument;

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
            Owner.BitmapManager.DocumentChanged += DocumentChanged;
            this.clientId = clientId;

            Enabled = PreferencesSettings.GetPreference<bool>("EnableRichPresence");
            PreferencesSettings.AddCallback("EnableRichPresence", x => Enabled = (bool)x);
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

        public void UpdatePresence(Document document)
        {
            RichPresence richPresence = new RichPresence
            {
                Assets = new Assets
                {
                    LargeImageKey = "editorlogo",
                    LargeImageText = "You discovered PixiEditor's logo",
                    SmallImageKey = "github",
                    SmallImageText = "Download PixiEditor on GitHub (please)!"
                }
            };

            if (document == null)
            {
                richPresence.WithTimestamps(new Timestamps(DateTime.UtcNow));
                richPresence.Details = "Staring at absolutely";
                richPresence.State = "nothing";
            }
            else
            {
                richPresence.WithTimestamps(new Timestamps(document.OpenedUTC));
                richPresence.Details = $"Editing {document.Name}";

                string state = $"{document.Width}x{document.Height}, ";

                if (document.Layers.Count == 1)
                {
                    state += "1 Layer";
                }
                else
                {
                    state += $"{document.Layers.Count} Layers";
                }

                richPresence.State = state;
            }

            client.SetPresence(richPresence);
        }

        private void DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
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

        private void DocumentLayerChanged(object sender, Models.Controllers.LayersChangedEventArgs e)
        {
            UpdatePresence(currentDocument);
        }

        private void DocumentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "Width" || e.PropertyName == "Height")
            {
                UpdatePresence(currentDocument);
            }
        }

        private void OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            UpdatePresence(Owner.BitmapManager.ActiveDocument);
        }

        ~DiscordViewModel()
        {
            Stop();
        }
    }
}