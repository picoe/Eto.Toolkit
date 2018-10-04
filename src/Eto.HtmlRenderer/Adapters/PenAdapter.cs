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

using Eto.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    /// <summary>
    /// Adapter for Eto pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual Eto brush instance.
        /// </summary>
        private readonly Pen _pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Pen pen)
        {
            _pen = pen;
        }

        /// <summary>
        /// The actual Eto brush instance.
        /// </summary>
        public Pen Pen
        {
            get { return _pen; }
        }

        public override double Width
        {
            get { return _pen.Thickness; }
            set { _pen.Thickness = (float)value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _pen.DashStyle = DashStyles.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.DashStyle = DashStyles.Dash;
                        break;
                    case RDashStyle.Dot:
                        _pen.DashStyle = DashStyles.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _pen.DashStyle = DashStyles.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _pen.DashStyle = DashStyles.DashDotDot;
                        break;
                    //case RDashStyle.Custom:
                    //    _pen.DashStyle = DashStyles.Custom;
                    //    break;
                    default:
                        _pen.DashStyle = DashStyles.Solid;
                        break;
                }
            }
        }
    }
}