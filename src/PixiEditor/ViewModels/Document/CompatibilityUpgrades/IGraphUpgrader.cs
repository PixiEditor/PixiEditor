using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.Document.CompatibilityUpgrades;

internal interface IGraphUpgrader
{
    event Action UpgradeCompleted;
    LocalizedString UpgradeText { get; }
    public void Upgrade();
}
