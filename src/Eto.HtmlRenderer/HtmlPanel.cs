// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.ComponentModel;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Eto.Adapters;
using TheArtOfDev.HtmlRenderer.Eto.Utilities;

namespace TheArtOfDev.HtmlRenderer.Eto
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// Eto control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// <see cref="MaximumSize"/> and <see cref="MinimumSize"/> with AutoSize can limit the max/min size of the control<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>AutoSize:</h4>
    /// <u>AutoSize = AutoSizeHeightOnly = false</u><br/>
    /// The label size will not change by the html content. MaximumSize and MinimumSize are ignored.<br/>
    /// <br/>
    /// <u>AutoSize = true</u><br/>
    /// The width and height is adjustable by the html content, the width will be longest line in the html, MaximumSize.Width will restrict it but it can be lower than that.<br/>
    /// <br/>
    /// <u>AutoSizeHeightOnly = true</u><br/>
    /// The width of the label is set and will not change by the content, the height is adjustable by the html content with restrictions to the MaximumSize.Height and MinimumSize.Height values.<br/>
    /// </para>
    /// <para>
    /// <h4>LinkClicked event</h4>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link.
    /// </para>
    /// <para>
    /// <h4>StylesheetLoad event:</h4>
    /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
    /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <h4>ImageLoad event:</h4>
    /// Raised when an image is about to be loaded by file path or URI.<br/>
    /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
    /// </para>
    /// <para>
    /// <h4>RenderError event:</h4>
    /// Raised when an error occurred during html rendering.<br/>
    /// </para>
    /// </summary>
    public class HtmlPanel : Drawable
    {
        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected HtmlContainer _htmlContainer;

        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        protected string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the panel
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// the current html text set in the control
        /// </summary>
        protected string _text;

        /// <summary>
        /// is to handle auto size of the control height only
        /// </summary>
        protected bool _autoSizeHight;

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        protected bool _useSystemCursors;

        #endregion


        /// <summary>
        /// Creates a new HTML Label
        /// </summary>
        public HtmlPanel()
        {
            _htmlContainer = new HtmlContainer();
            _htmlContainer.AvoidImagesLateLoading = true;
            _htmlContainer.MaxSize = MaximumSize;
            _htmlContainer.LoadComplete += OnHtmlLoadComplete;
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
        }

        /// <summary>
        ///   Raised when the BorderStyle property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler HtmlLoadComplete;

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If anti-aliasing should be avoided for geometry like backgrounds and borders")]
        public virtual bool AvoidGeometryAntialias
        {
            get { return _htmlContainer.AvoidGeometryAntialias; }
            set { _htmlContainer.AvoidGeometryAntialias = value; }
        }

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        [Description("If to use cursors defined by the operating system or .NET cursors")]
        public bool UseSystemCursors
        {
            get { return _useSystemCursors; }
            set { _useSystemCursors = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is content selection is enabled for the rendered html.")]
        public virtual bool IsSelectionEnabled
        {
            get { return _htmlContainer.IsSelectionEnabled; }
            set { _htmlContainer.IsSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is the build-in context menu enabled and will be shown on mouse right click.")]
        public virtual bool IsContextMenuEnabled
        {
            get { return _htmlContainer.IsContextMenuEnabled; }
            set { _htmlContainer.IsContextMenuEnabled = value; }
        }

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Browsable(true)]
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        [Category("Appearance")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public virtual string BaseStylesheet
        {
            get { return _baseRawCssData; }
            set
            {
                _baseRawCssData = value;
                _baseCssData = HtmlRender.ParseStyleSheet(value);
                _htmlContainer.SetHtml(_text, _baseCssData);
            }
        }

        /// <summary>
        /// Automatically sets the height of the label by content height (width is not effected).
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Layout")]
        [Description("Automatically sets the height of the label by content height (width is not effected)")]
        public virtual bool AutoSizeHeightOnly
        {
            get { return _autoSizeHight; }
            set
            {
                _autoSizeHight = value;
                if (value)
                {
                    //AutoSize = false;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the html of this control.
        /// </summary>
        [Description("Sets the html of this control.")]
        public virtual string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _htmlContainer.SetHtml(_text, _baseCssData);
                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedText
        {
            get { return _htmlContainer.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedHtml
        {
            get { return _htmlContainer.SelectedHtml; }
        }

        /// <summary>
        /// Get html from the current DOM tree with inline style.
        /// </summary>
        /// <returns>generated html</returns>
        public virtual string GetHtml()
        {
            return _htmlContainer != null ? _htmlContainer.GetHtml() : null;
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public virtual RectangleF? GetElementRectangle(string elementId)
        {
            return _htmlContainer != null ? _htmlContainer.GetElementRectangle(elementId) : null;
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_htmlContainer != null)
                _htmlContainer.ClearSelection();
        }

        public Size MaximumSize { get; set; } = Size.MaxValue;


        #region Private methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected void PerformLayout()
        {
            if (_htmlContainer != null)
            {
                using (var g = Utils.CreateGraphics())
                {
                    using (var ig = new GraphicsAdapter(g))
                    {
                        var newSize = HtmlRendererUtils.Layout(ig,
                            _htmlContainer.HtmlContainerInt,
                            new RSize(ClientSize.Width - Padding.Horizontal, ClientSize.Height - Padding.Vertical),
                            new RSize(MinimumSize.Width - Padding.Horizontal, MinimumSize.Height - Padding.Vertical),
                            new RSize(MaximumSize.Width - Padding.Horizontal, MaximumSize.Height - Padding.Vertical),
                            true,
                            false);
                        ClientSize = Utils.ConvertRound(new RSize(newSize.Width + Padding.Horizontal, newSize.Height + Padding.Vertical));
                    }
                }
            }
        }

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_htmlContainer != null)
            {
                _htmlContainer.Location = new PointF(Padding.Left, Padding.Top);
                _htmlContainer.PerformPaint(e.Graphics);
            }
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection. 
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseMove(this, e);
        }

        /// <summary>
        /// Handle mouse down to handle selection. 
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDown(this, e);
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click. 
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseUp(this, e);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse. 
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDoubleClick(this, e);
        }

        /// <summary>
        ///   Raises the <see cref="BorderStyleChanged" /> event.
        /// </summary>
        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            //UpdateStyles();

            var handler = BorderStyleChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the LoadComplete event from root container.
        /// </summary>
        protected virtual void OnHtmlLoadComplete(EventArgs e)
        {
            var handler = HtmlLoadComplete;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e)
        {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e)
        {
            var handler = RenderError;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e)
        {
            var handler = StylesheetLoad;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e)
        {
            var handler = ImageLoad;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e)
        {
            if (e.Layout)
                PerformLayout();
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _htmlContainer?.HandleKeyDown(this, e);
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.LoadComplete -= OnHtmlLoadComplete;
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
            base.Dispose(disposing);
        }


        #region Private event handlers

        private void OnHtmlLoadComplete(object sender, EventArgs e)
        {
            OnLoadComplete(e);
        }

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }

        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            Application.Instance.Invoke(() => OnRenderError(e));
        }

        private void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            OnStylesheetLoad(e);
        }

        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            OnImageLoad(e);
        }

        private void OnRefresh(object sender, HtmlRefreshEventArgs e)
        {
            Application.Instance.Invoke(() => OnRefresh(e));
        }

        #endregion


        #endregion
    }
}