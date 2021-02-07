using System.Windows;

namespace PixiEditor.Styles.AvalonDock.Themes
{
    /// <summary>
	/// Resource key management class to keep track of all resources
    /// that can be re-styled in applications that make use of the implemented controls.
	/// </summary>
	public static class ResourceKeys
	{
		#region Accent Keys
		/// <summary>
        /// Accent Color Key - This Color key is used to accent elements in the UI
		/// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
		/// </summary>
		public static readonly ComponentResourceKey ControlAccentColorKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlAccentColorKey");

		/// <summary>
		/// Accent Brush Key - This Brush key is used to accent elements in the UI
		/// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
		/// </summary>
		public static readonly ComponentResourceKey ControlAccentBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlAccentBrushKey");
		#endregion Accent Keys

		#region Brush Keys
		// General
		public static readonly ComponentResourceKey Background = new ComponentResourceKey(typeof(ResourceKeys), "Background");
		public static readonly ComponentResourceKey PanelBorderBrush = new ComponentResourceKey(typeof(ResourceKeys), "PanelBorderBrush");
		public static readonly ComponentResourceKey TabBackground = new ComponentResourceKey(typeof(ResourceKeys), "TabBackground");

		// Auto Hide : Tab
		public static readonly ComponentResourceKey AutoHideTabDefaultBackground = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultBackground");
		public static readonly ComponentResourceKey AutoHideTabDefaultBorder = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultBorder");
		public static readonly ComponentResourceKey AutoHideTabDefaultText = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultText");

		// Mouse Over Auto Hide Button for (collapsed) Auto Hidden Elements
		public static readonly ComponentResourceKey AutoHideTabHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredBackground");
		// AccentColor
		public static readonly ComponentResourceKey AutoHideTabHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredBorder");
		// AccentColor
		public static readonly ComponentResourceKey AutoHideTabHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredText");

		// Document Well : Overflow Button
		public static readonly ComponentResourceKey DocumentWellOverflowButtonDefaultGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonDefaultGlyph");
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredBackground");
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredBorder");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredGlyph");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedBackground");
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedBorder");
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedGlyph");

		// Document Well : Tab
		// Selected Document Highlight Header Top color (AccentColor)
		public static readonly ComponentResourceKey DocumentWellTabSelectedActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedActiveBackground");

		public static readonly ComponentResourceKey DocumentWellTabSelectedActiveText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedActiveText");
		public static readonly ComponentResourceKey DocumentWellTabSelectedInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedInactiveBackground");
		public static readonly ComponentResourceKey DocumentWellTabSelectedInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedInactiveText");
		public static readonly ComponentResourceKey DocumentWellTabUnselectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedBackground");
		public static readonly ComponentResourceKey DocumentWellTabUnselectedText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedText");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabUnselectedHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedHoveredBackground");
		public static readonly ComponentResourceKey DocumentWellTabUnselectedHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedHoveredText");

		// Document Well : Tab : Button
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveGlyph");

		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredBackground");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredGlyph");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedBackground");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedGlyph");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveGlyph");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredBackground");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredGlyph");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedBackground");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedGlyph");
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredGlyph");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground");
		// AccentColor
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder");
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph");

		// Tool Window : Caption
		// Background of selected toolwindow (AccentColor)
		public static readonly ComponentResourceKey ToolWindowCaptionActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveBackground");
		public static readonly ComponentResourceKey ToolWindowCaptionActiveGrip = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveGrip");
		public static readonly ComponentResourceKey ToolWindowCaptionActiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveText");
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveBackground");
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveGrip = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveGrip");
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveText");

		// Tool Window : Caption : Button
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveGlyph");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredBackground");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredBorder");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredGlyph");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedBackground");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedBorder");

		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedGlyph");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveGlyph");

		// AccentColor
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredBackground");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredBorder");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredGlyph");

		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedBackground");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedBorder");
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedGlyph");

		// Tool Window : Tab
		public static readonly ComponentResourceKey ToolWindowTabSelectedActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedActiveBackground");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowTabSelectedActiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedActiveText");
		public static readonly ComponentResourceKey ToolWindowTabSelectedInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedInactiveBackground");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowTabSelectedInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedInactiveText");
		public static readonly ComponentResourceKey ToolWindowTabUnselectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedBackground");
		public static readonly ComponentResourceKey ToolWindowTabUnselectedText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedText");
		public static readonly ComponentResourceKey ToolWindowTabUnselectedHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedHoveredBackground");
		// AccentColor
		public static readonly ComponentResourceKey ToolWindowTabUnselectedHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedHoveredText");

		// Floating Document Window
		public static readonly ComponentResourceKey FloatingDocumentWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "FloatingDocumentWindowBackground");
		public static readonly ComponentResourceKey FloatingDocumentWindowBorder = new ComponentResourceKey(typeof(ResourceKeys), "FloatingDocumentWindowBorder");

		// Floating Tool Window
		public static readonly ComponentResourceKey FloatingToolWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "FloatingToolWindowBackground");
		public static readonly ComponentResourceKey FloatingToolWindowBorder = new ComponentResourceKey(typeof(ResourceKeys), "FloatingToolWindowBorder");

		// Navigator Window
		public static readonly ComponentResourceKey NavigatorWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowBackground");
		public static readonly ComponentResourceKey NavigatorWindowForeground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowForeground");

		// Background of selected text in Navigator Window (AccentColor)
		public static readonly ComponentResourceKey NavigatorWindowSelectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowSelectedBackground");
		public static readonly ComponentResourceKey NavigatorWindowSelectedText = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowSelectedText");

		#region DockingBrushKeys
		// Defines the height and width of the docking indicator buttons that are shown when
		// documents or tool windows are dragged
		public static readonly ComponentResourceKey DockingButtonWidthKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonWidthKey");
		public static readonly ComponentResourceKey DockingButtonHeightKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonHeightKey");

		public static readonly ComponentResourceKey DockingButtonBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonBackgroundBrushKey");
		public static readonly ComponentResourceKey DockingButtonForegroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonForegroundBrushKey");
		public static readonly ComponentResourceKey DockingButtonForegroundArrowBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonForegroundArrowBrushKey");

		public static readonly ComponentResourceKey DockingButtonStarBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonStarBorderBrushKey");
		public static readonly ComponentResourceKey DockingButtonStarBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonStarBackgroundBrushKey");

		// Preview Box is the highlighted rectangle that shows when a drop area in a window is indicated
		public static readonly ComponentResourceKey PreviewBoxBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "PreviewBoxBorderBrushKey");
		public static readonly ComponentResourceKey PreviewBoxBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "PreviewBoxBackgroundBrushKey");
		#endregion DockingBrushKeys
		#endregion Brush Keys
	}
}