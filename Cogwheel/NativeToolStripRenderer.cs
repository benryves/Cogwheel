using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;

namespace Asztal.Szótár {
	/// <summary>
	/// Renders a toolstrip using the UxTheme API via VisualStyleRenderer. Visual styles must be supported for this to work; if you need to support other operating systems use
	/// </summary>
	class UXThemeToolStripRenderer : ToolStripRenderer {
		/// <summary>
		/// It shouldn't be necessary to P/Invoke like this, however a bug in VisualStyleRenderer.GetMargins forces my hand.
		/// </summary>
		static internal class NativeMethods {
			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct MARGINS {
				public int cxLeftWidth;
				public int cxRightWidth;
				public int cyTopHeight;
				public int cyBottomHeight;
			}

			[DllImport("uxtheme", ExactSpelling = true)]
			public extern static Int32 GetThemeMargins(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, IntPtr rect, out MARGINS pMargins);
		}

		//See http://msdn2.microsoft.com/en-us/library/bb773210.aspx - "Parts and States"
		#region Parts and States 
		enum MenuParts : int {
			MENU_MENUITEM_TMSCHEMA = 1,
			MENU_MENUDROPDOWN_TMSCHEMA = 2,
			MENU_MENUBARITEM_TMSCHEMA = 3,
			MENU_MENUBARDROPDOWN_TMSCHEMA = 4,
			MENU_CHEVRON_TMSCHEMA = 5,
			MENU_SEPARATOR_TMSCHEMA = 6,
			MENU_BARBACKGROUND = 7,
			MENU_BARITEM = 8,
			MENU_POPUPBACKGROUND = 9,
			MENU_POPUPBORDERS = 10,
			MENU_POPUPCHECK = 11,
			MENU_POPUPCHECKBACKGROUND = 12,
			MENU_POPUPGUTTER = 13,
			MENU_POPUPITEM = 14,
			MENU_POPUPSEPARATOR = 15,
			MENU_POPUPSUBMENU = 16,
			MENU_SYSTEMCLOSE = 17,
			MENU_SYSTEMMAXIMIZE = 18,
			MENU_SYSTEMMINIMIZE = 19,
			MENU_SYSTEMRESTORE = 20
		}

		enum MenuBarStates : int {
			MB_ACTIVE = 1,
			MB_INACTIVE = 2
		}

		enum MenuBarItemStates : int {
			MBI_NORMAL = 1,
			MBI_HOT = 2,
			MBI_PUSHED = 3,
			MBI_DISABLED = 4,
			MBI_DISABLEDHOT = 5,
			MBI_DISABLEDPUSHED = 6
		}

		enum MenuPopupItemStates : int {
			MPI_NORMAL = 1,
			MPI_HOT = 2,
			MPI_DISABLED = 3,
			MPI_DISABLEDHOT = 4
		}

		enum MenuPopupCheckStates : int {
			MC_CHECKMARKNORMAL = 1,
			MC_CHECKMARKDISABLED = 2,
			MC_BULLETNORMAL = 3,
			MC_BULLETDISABLED = 4
		}

		enum MenuPopupCheckBackgroundStates : int {
			MCB_DISABLED = 1,
			MCB_NORMAL = 2,
			MCB_BITMAP = 3
		}

		enum MenuPopupSubMenuStates : int {
			MSM_NORMAL = 1,
			MSM_DISABLED = 2
		}

		enum MarginTypes : int {
			TMT_SIZINGMARGINS = 3601,
			TMT_CONTENTMARGINS = 3602,
			TMT_CAPTIONMARGINS = 3603
		}
		#endregion

		VisualStyleRenderer renderer;
		private static string VSCLASS_MENU = "MENU";

		public UXThemeToolStripRenderer() {
			renderer = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);
		}

		protected override void Initialize(ToolStrip toolStrip) {
			base.Initialize(toolStrip);
		}

		protected override void InitializeItem(ToolStripItem item) {
			base.InitializeItem(item);
		}

		private static int GetItemState(ToolStripItem item) {
			bool pressed = item.Pressed;
			bool hot = item.Selected;

			if (item.Owner.IsDropDown) {
				if (item.Enabled)
					return hot ? (int)MenuPopupItemStates.MPI_HOT : (int)MenuPopupItemStates.MPI_NORMAL;
				return hot ? (int)MenuPopupItemStates.MPI_DISABLEDHOT : (int)MenuPopupItemStates.MPI_DISABLED;
			} else {
				if (pressed)
					return item.Enabled ? (int)MenuBarItemStates.MBI_PUSHED : (int)MenuBarItemStates.MBI_DISABLEDPUSHED;
				if (item.Enabled)
					return hot ? (int)MenuBarItemStates.MBI_HOT : (int)MenuBarItemStates.MBI_NORMAL;
				return hot ? (int)MenuBarItemStates.MBI_DISABLEDHOT : (int)MenuBarItemStates.MBI_DISABLED;
			}
		}

		protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e) {
			if (e.ToolStrip.IsDropDown)
				renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPBACKGROUND, 0);
			else
				renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_BARBACKGROUND, e.ToolStrip.Enabled ? (int)MenuBarStates.MB_ACTIVE : (int)MenuBarStates.MB_INACTIVE);

			renderer.DrawBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.AffectedBounds);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
			renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPBORDERS, 0);
			if (e.ToolStrip.IsDropDown) {
				Region oldClip = e.Graphics.Clip;

				//Tool strip borders are rendered *after* the content, for some reason.
				//So we have to exclude the inside of the popup otherwise we'll draw over it.
				Rectangle insideRect = e.ToolStrip.ClientRectangle;
				insideRect.Inflate(-1, -1);
				e.Graphics.ExcludeClip(insideRect);

				renderer.DrawBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.AffectedBounds);

				//Restore the old clip in case the Graphics is used again (does that ever happen?)
				e.Graphics.Clip = oldClip;
			}
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
			int partId = e.Item.Owner.IsDropDown ? (int)MenuParts.MENU_POPUPITEM : (int)MenuParts.MENU_BARITEM;
			renderer.SetParameters(VSCLASS_MENU, partId, GetItemState(e.Item));
			
			Rectangle bgRect = e.Item.ContentRectangle;

			if (!e.Item.Owner.IsDropDown) {
				bgRect.Y = 0;
				bgRect.Height = e.ToolStrip.Height;
				bgRect.Inflate(-1, 0); //WHY?!?!? Oh well, it seems to fix things.
			}

			renderer.DrawBackground(e.Graphics, bgRect, bgRect);
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
			int partId = e.Item.Owner.IsDropDown ? (int)MenuParts.MENU_POPUPITEM : (int)MenuParts.MENU_BARITEM;
			renderer.SetParameters(VSCLASS_MENU, partId, GetItemState(e.Item));
			Color color = renderer.GetColor(ColorProperty.TextColor);

			e.TextColor = color;
			base.OnRenderItemText(e);
		}

		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e) {
			if (e.GripStyle == ToolStripGripStyle.Visible) {
				renderer.SetParameters(VisualStyleElement.Rebar.Gripper.Normal);
				renderer.DrawBackground(e.Graphics, e.GripBounds, e.AffectedBounds);
			}
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
			if (e.ToolStrip.IsDropDown) {
				renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPGUTTER, 0);
				Rectangle displayRect = e.ToolStrip.DisplayRectangle,
					marginRect = new Rectangle(0, displayRect.Top, displayRect.Left, displayRect.Height);
				//e.Graphics.DrawRectangle(Pens.Black, marginRect);
				renderer.DrawBackground(e.Graphics, marginRect, marginRect);
			}
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
			int partId = e.Item.Owner.IsDropDown ? (int)MenuParts.MENU_POPUPSEPARATOR : (int)MenuParts.MENU_SEPARATOR_TMSCHEMA;
			renderer.SetParameters(VSCLASS_MENU, partId, 0);
			Rectangle rect = new Rectangle(e.ToolStrip.DisplayRectangle.Left, 0, e.ToolStrip.DisplayRectangle.Width, e.Item.Height);
			renderer.DrawBackground(e.Graphics, rect, rect);
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
			ToolStripMenuItem item = e.Item as ToolStripMenuItem;
			if (item != null) {
				if (item.Checked) {
					Rectangle rect = e.Item.ContentRectangle;
					rect.Width = rect.Height;

					//Center the checkmark horizontally in the gutter (looks ugly, though)
					//rect.X = (e.ToolStrip.DisplayRectangle.Left - rect.Width) / 2;

					renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPCHECKBACKGROUND, e.Item.Enabled ? (int)MenuPopupCheckBackgroundStates.MCB_NORMAL : (int)MenuPopupCheckBackgroundStates.MCB_DISABLED);
					renderer.DrawBackground(e.Graphics, rect);
					
					//This crashes. Blame WinForms, they got the p/invoke wrong.
					//Padding padding = renderer.GetMargins(e.Graphics, MarginProperty.ContentMargins);

					try {
						NativeMethods.MARGINS margins = new NativeMethods.MARGINS();
						int hresult = NativeMethods.GetThemeMargins(renderer.Handle, e.Graphics.GetHdc(), renderer.Part, renderer.State, (int)MarginTypes.TMT_SIZINGMARGINS, IntPtr.Zero, out margins);

						if (hresult == 0) { //S_OK
							rect = new Rectangle(rect.X + margins.cxLeftWidth, rect.Y + margins.cyTopHeight,
							rect.Width - margins.cxLeftWidth - margins.cxRightWidth,
							rect.Height - margins.cyBottomHeight - margins.cyTopHeight);
						}
					} finally {
						e.Graphics.ReleaseHdc();
					}

					//I don't think ToolStrip even supports radio box items. So no need to render them.
					renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPCHECK, e.Item.Enabled ? (int)MenuPopupCheckStates.MC_CHECKMARKNORMAL : (int)MenuPopupCheckStates.MC_CHECKMARKDISABLED);

					renderer.DrawBackground(e.Graphics, rect);
				}
			} else {
				base.OnRenderItemCheck(e);
			}
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
			int stateId = e.Item.Enabled ? (int)MenuPopupSubMenuStates.MSM_NORMAL : (int)MenuPopupSubMenuStates.MSM_DISABLED;
			renderer.SetParameters(VSCLASS_MENU, (int)MenuParts.MENU_POPUPSUBMENU, stateId);
			renderer.DrawBackground(e.Graphics, e.ArrowRectangle);
		}

		public static bool IsSupported {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return false;

				return VisualStyleRenderer.IsElementDefined(VisualStyleElement.CreateElement(VSCLASS_MENU, (int)MenuParts.MENU_BARBACKGROUND, (int)MenuBarStates.MB_ACTIVE));
			} 
		}
	}

	/// <summary>
	/// Renders a toolstrip using UXTheme if possible, and switches back to the default
	/// ToolStripRenderer when UXTheme-based rendering is not available.
	/// Designed for menu bars and context menus - it is not guaranteed to work with anything else.
	/// </summary>
	/// <example>
	/// NativeToolStripRenderer.SetToolStripRenderer(toolStrip1, toolStrip2, contextMenuStrip1);
	/// </example>
	/// <example>
	/// toolStrip1.Renderer = new NativeToolStripRenderer();
	/// </example>
	public class NativeToolStripRenderer : ToolStripRenderer {
		UXThemeToolStripRenderer nativeRenderer;
		ToolStripRenderer defaultRenderer;
		ToolStrip toolStrip;

		//NativeToolStripRenderer looks best with no padding - but keep the old padding in case the
		//visual styles become unsupported again (e.g. user changes to windows classic skin)
		Padding defaultPadding;

		/// <summary>
		/// Creates a NativeToolStripRenderer for a particular ToolStrip. NativeToolStripRenderer  will subscribe to some events
		/// of this ToolStrip.
		/// </summary>
		/// <param name="toolStrip">The toolstrip for this NativeToolStripRenderer. NativeToolStripRenderer  will subscribe to some events
		/// of this ToolStrip.</param>
		public NativeToolStripRenderer(ToolStrip toolStrip) {
			if (toolStrip == null)
				throw new ArgumentNullException("toolStrip", "ToolStrip cannot be null.");

			this.toolStrip = toolStrip;
			defaultRenderer = toolStrip.Renderer;

			defaultPadding = toolStrip.Padding;
			toolStrip.SystemColorsChanged += new EventHandler(toolStrip_SystemColorsChanged);

			//Can't initialize here - constructor throws if visual styles not enabled
			//nativeRenderer = new NativeToolStripRenderer();
		}

		void toolStrip_SystemColorsChanged(object sender, EventArgs e) {
			if (UXThemeToolStripRenderer.IsSupported)
				toolStrip.Padding = Padding.Empty;
			else
				toolStrip.Padding = defaultPadding;
		}

		//This is indeed called every time a menu part is rendered, but I can't
		//find a way of caching it that I can be sure has no race conditions.
		//The check is no longer very costly, anyway.
		protected ToolStripRenderer ActualRenderer {
			get {
				bool nativeSupported = UXThemeToolStripRenderer.IsSupported;
				
				if (nativeSupported) {
					if (nativeRenderer == null)
						nativeRenderer = new UXThemeToolStripRenderer();
					return nativeRenderer;
				}

				return defaultRenderer;
			}
		}

		protected override void Initialize(ToolStrip toolStrip) {
			base.Initialize(toolStrip);

			toolStrip.Padding = Padding.Empty;
		}

		protected override void InitializeItem(ToolStripItem item) {
			base.InitializeItem(item);
		}

		/// <summary>
		/// Sets the renderer of each ToolStrip to a NativeToolStripRenderer. A convenience method.
		/// </summary>
		/// <param name="toolStrips">A parameter list of ToolStrips.</param>
		[SuppressMessage("Microsoft.Design", "CA1062")] //The parameter array is actually checked.
		public static void SetToolStripRenderer(params ToolStrip[] toolStrips) {
			foreach (ToolStrip ts in toolStrips) {
				if (ts == null)
					throw new ArgumentNullException("toolStrips", "ToolStrips cannot contain a null reference.");

				ts.Renderer = new NativeToolStripRenderer(ts);
			}
		}

		#region Overridden Methods - Deferred to actual renderer
		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
			ActualRenderer.DrawArrow(e);
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawButtonBackground(e);
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawDropDownButtonBackground(e);
		}

		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e) {
			ActualRenderer.DrawGrip(e);
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
			ActualRenderer.DrawImageMargin(e);
		}

		protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawItemBackground(e);
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
			ActualRenderer.DrawItemCheck(e);
		}

		protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
			ActualRenderer.DrawItemImage(e);
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
			ActualRenderer.DrawItemText(e);
		}

		protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawLabelBackground(e);
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawMenuItemBackground(e);
		}

		protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawOverflowButtonBackground(e);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
			ActualRenderer.DrawSeparator(e);
		}

		//Not sure why the inconsistency here... oh well.
		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawSplitButton(e);
		}

		protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e) {
			ActualRenderer.DrawStatusStripSizingGrip(e);
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
			ActualRenderer.DrawToolStripBackground(e);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
			ActualRenderer.DrawToolStripBorder(e);
		}

		protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e) {
			ActualRenderer.DrawToolStripContentPanelBackground(e);
		}

		protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e) {
			ActualRenderer.DrawToolStripPanelBackground(e);
		}

		protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e) {
			ActualRenderer.DrawToolStripStatusLabelBackground(e);
		}
		#endregion
	}
}
