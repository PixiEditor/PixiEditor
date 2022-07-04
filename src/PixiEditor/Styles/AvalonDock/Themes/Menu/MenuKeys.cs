using System.Windows;

namespace PixiEditor.Styles.AvalonDock.Themes.Menu
{
    /// <summary>
    /// Class implements static resource keys that should be referenced to configure
    /// menu specific colors, styles and other elements that are typically changed
    /// between themes.
    /// </summary>
    public static class MenuKeys
    {
        /// <summary>
        /// Gets the Brush key for the normal Menu separator border color.
        /// </summary>
        public static readonly ComponentResourceKey MenuSeparatorBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuSeparatorBorderBrushKey");

        /// <summary>
        /// Gets the Brush key for the normal background of a Sub-Menu-Item.
        /// </summary>
        public static readonly ComponentResourceKey SubmenuItemBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundKey");

        /// <summary>
        /// Gets the Brush key for highlighting the background of a Menu-Item on mouse over.
        /// </summary>
        public static readonly ComponentResourceKey MenuItemHighlightedBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuItemHighlightedBackgroundKey");

        /// <summary>
        /// Gets the Brush key for highlighting the background of a Menu-Item on mouse over.
        /// </summary>
        public static readonly ComponentResourceKey SubmenuItemBackgroundHighlightedKey = new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundHighlightedKey");

        // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        /// <summary>
        /// Gets the background Brush key for a Menu Repeat button in IsPressed state.
        /// (see context menu below)
        /// </summary>
        public static readonly ComponentResourceKey FocusScrollButtonBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "FocusScrollButtonBrushKey");

        /// <summary>
        /// Gets the background Brush key for a Context-Menu Repeat button in IsPressed state.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey ScrollButtonBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "ScrollButtonBrushKey");

        /// <summary>
        /// Gets the background Brush key of a Checkmark in a menu or context menu.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey CheckMarkBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBackgroundBrushKey");

        /// <summary>
        /// Gets the border Brush key of a Checkmark in a menu or context menu.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey CheckMarkBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBorderBrushKey");

        /// <summary>
        /// Gets the foreground Brush key of a Checkmark in a menu or context menu.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey CheckMarkForegroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkForegroundBrushKey");

        /// <summary>
        /// Gets the background Brush key of a disabled sub-menu-item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey DisabledSubMenuItemBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBackgroundBrushKey");

        /// <summary>
        /// Gets the border Brush key of a disabled sub-menu-item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey DisabledSubMenuItemBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBorderBrushKey");

        /// <summary>
        /// Gets the border Brush key of a disabled sub-menu-item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey MenuBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuBorderBrushKey");

        /// <summary>
        /// Gets the normal background Brush key of a menu.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey MenuBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuBackgroundKey");

        /// <summary>
        /// Gets the normal background Brush key of the top level item in a menu (Files, Edit, ...).
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey TopLevelHeaderMenuBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "TopLevelHeaderMenuBackgroundKey");

        /// <summary>
        /// Gets the normal text or foreground Brush key of a menu item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey TextBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "TextBrushKey");

        /// <summary>
        /// Gets the normal background Brush key of a selected menu item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey ItemBackgroundSelectedKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundSelectedKey");

        /// <summary>
        /// Gets the text or foreground Brush key of a menu item in disabled state.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey ItemTextDisabledKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemTextDisabledKey");

        /// <summary>
        /// Gets the normal background Brush key of a menu item.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey NormalBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "NormalBackgroundBrushKey");

        /// <summary>
        /// Gets the background Brush key of a menu item in mouse over state.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey ItemBackgroundHoverKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundHoverKey");

        /// <summary>
        /// Gets the Brush key that is applied to draw a drop shadow (if any) below a menu.
        /// (see menu above)
        /// </summary>
        public static readonly ComponentResourceKey DropShadowEffectKey = new ComponentResourceKey(typeof(ResourceKeys), "DropShadowEffectKey");
    }
}
