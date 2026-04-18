using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveMarkdown.Avalonia;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class AvailableContentViewModel : ObservableObject
{
    public AvailableContent AvailableContent { get; }

    public bool IsOwned => extensionManager.IsExtensionOwned(AvailableContent.Id);

    public bool IsBundle => AvailableContent.IsBundle;

    public bool AllBundleItemsOwned =>
        IsBundle && AvailableContent.IncludedExtensions.All(id => extensionManager.IsExtensionOwned(id));

    public bool IsCountryUnsupported => Currency == "UNSUPPORTED";

    public string PriceText => IsOwned
        ? "EXTENSIONS_WINDOW_IN_LIBRARY"
        : (
            AllBundleItemsOwned ? "EXTENSIONS_WINDOW_ALL_FROM_BUNDLE_OWNED" : CalculatedPrice
        );

    public string CalculatedPrice
    {
        get
        {
            if (IsUnavailable)
            {
                return "";
            }

            if (AvailableContent.Price == 0 && !IsBundle)
            {
                return "FREE";
            }

            if (IsCountryUnsupported)
                return "UNAVAILABLE_IN_YOUR_COUNTRY";

            double price = AvailableContent.Price;

            if (AvailableContent.IsBundle)
            {
                price = 0;
                foreach (var ext in AvailableContent.IncludedExtensions)
                {
                    if (!extensionManager.IsExtensionOwned(ext))
                    {
                        var extInfo =
                            extensionManager.AvailableExtensions.FirstOrDefault(e => e.AvailableContent.Id == ext);
                        if (extInfo != null)
                        {
                            price += extInfo.AvailableContent.Price;
                        }
                    }
                }

                price = price * (1 - AvailableContent.PercentageDiscount / 100.0);
            }

            if (Currency != "PLN")
            {
                return $"{((price / Rate) * 1.04):0.00} {Currency}";
            }

            return $"{price:0.00} {Currency}";
        }
    }

    private readonly ExtensionManagerViewModel extensionManager;

    private double Rate { get; }
    private string Currency { get; }
    public bool IsFree => AvailableContent.Price == 0 && !IsBundle;
    public ObservableStringBuilder MarkdownBody { get; } = new ObservableStringBuilder();
    public bool IsUnavailable { get; }
    public ObservableCollection<ShowcaseItem> ShowcaseItems { get; } = new ObservableCollection<ShowcaseItem>();

    private HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

    public AvailableContentViewModel(AvailableContent content, ExtensionManagerViewModel extensionManager, double rate,
        string currency, bool isUnavailable)
    {
        AvailableContent = content;
        this.extensionManager = extensionManager;
        Rate = rate;
        Currency = currency;
        if (Uri.TryCreate(AvailableContent.Body, UriKind.Absolute, out Uri bodyUri))
        {
            FetchContentFromUri(bodyUri);
        }
        else
        {
            MarkdownBody.Append(content.Body);
        }

        IsUnavailable = isUnavailable;
        if (content.ShowcaseUrls != null)
        {
            foreach (var showcaseItem in content.ShowcaseUrls)
            {
                if (Uri.TryCreate(showcaseItem, UriKind.Absolute, out Uri showcaseUri))
                {
                    bool isVideo = showcaseUri.AbsolutePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                   showcaseUri.AbsolutePath.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);
                    if (isVideo)
                    {
                        ShowcaseItems.Add(new VideoShowcaseItem(showcaseItem));
                    }
                    else
                    {
                        ShowcaseItems.Add(new ImageShowcaseItem(showcaseItem));
                    }
                }
            }
        }
    }

    private void FetchContentFromUri(Uri bodyUri)
    {
        Task.Run(async () =>
        {
            try
            {
                string markdown = await httpClient.GetStringAsync(bodyUri);
                Dispatcher.UIThread.Post(() => MarkdownBody.Append(markdown));
            }
            catch (Exception)
            {
                Dispatcher.UIThread.Post(() => MarkdownBody.Append("Failed to load content."));
            }
        });
    }

    public void NotifyChanged()
    {
        OnPropertyChanged(nameof(IsOwned));
        OnPropertyChanged(nameof(IsBundle));
        OnPropertyChanged(nameof(AllBundleItemsOwned));
        OnPropertyChanged(nameof(CalculatedPrice));
        OnPropertyChanged(nameof(PriceText));
        OnPropertyChanged(nameof(IsCountryUnsupported));
    }
}
