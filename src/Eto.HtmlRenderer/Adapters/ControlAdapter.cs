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

using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Eto.Utilities;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    /// <summary>
    /// Adapter for Eto Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline win forms control.
        /// </summary>
        private readonly Control _control;


        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control)
            : base(EtoAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            _control = control;
        }

        /// <summary>
        /// Get the underline win forms control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        public override RPoint MouseLocation => Utils.Convert(_control.PointFromScreen(Mouse.Position));

        public override bool LeftMouseButton => Mouse.Buttons.HasFlag(MouseButtons.Primary);

        public override bool RightMouseButton => Mouse.Buttons.HasFlag(MouseButtons.Alternate);

        public override void SetCursorDefault()
        {
            _control.Cursor = Cursors.Default;
        }

        public override void SetCursorHand()
        {
            _control.Cursor = Cursors.Move;
        }

        public override void SetCursorIBeam()
        {
            _control.Cursor = Cursors.IBeam;
        }

        public override void DoDragDropCopy(object dragDropData)
        {
            _control.DoDragDrop((DataObject)dragDropData, DragEffects.Copy);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            var realFont = ((FontAdapter)font).Font;
            charFit = 0;
            charFitWidth = 0;

            var size = realFont.MeasureString(str);

            for (int i = 1; i <= str.Length; i++)
            {
                charFit = i - 1;
                var pSize = realFont.MeasureString(str.Substring(0, i));
                if (pSize.Height <= size.Height && pSize.Width < maxWidth)
                    charFitWidth = pSize.Width;
                else
                    break;
            }
        }

        public override void Invalidate()
        {
            _control.Invalidate();
        }
    }
}