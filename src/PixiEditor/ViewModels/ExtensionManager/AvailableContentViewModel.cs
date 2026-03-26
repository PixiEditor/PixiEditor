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
            if (IsPurchaseUnavailableOnSteam)
            {
                return "";
            }
            
            if (AvailableContent.Price == 0)
            {
                return "FREE";
            }

            if (IsCountryUnsupported)
                return "UNAVAILABLE_IN_YOUR_COUNTRY";

            double price = AvailableContent.Price;

            if (AvailableContent.IncludedExtensions.Count > 0)
            {
                int ownedCount = AvailableContent.IncludedExtensions
                    .Count(id => extensionManager.IsExtensionOwned(id));

                price = (price - ((price / AvailableContent.IncludedExtensions.Count) * ownedCount));
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
    public bool IsFree => AvailableContent.Price == 0;
    public ObservableStringBuilder MarkdownBody { get; } = new ObservableStringBuilder();
    public bool IsPurchaseUnavailableOnSteam { get; }

    public AvailableContentViewModel(AvailableContent content, ExtensionManagerViewModel extensionManager, double rate,
        string currency, bool isPurchaseUnavailableOnSteam)
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
        
        IsPurchaseUnavailableOnSteam = isPurchaseUnavailableOnSteam;
    }

    private void FetchContentFromUri(Uri bodyUri)
    {
        Task.Run(async () =>
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
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
