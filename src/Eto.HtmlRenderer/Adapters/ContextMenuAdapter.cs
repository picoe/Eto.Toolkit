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
using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Eto.Utilities;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    /// <summary>
    /// Adapter for Eto context menu for core.
    /// </summary>
    internal sealed class ContextMenuAdapter : RContextMenu
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win forms context menu
        /// </summary>
        private readonly ContextMenu _contextMenu;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public ContextMenuAdapter()
        {
            _contextMenu = new ContextMenu();
        }

        public override int ItemsCount
        {
            get { return _contextMenu.Items.Count; }
        }

        public override void AddDivider()
        {
            _contextMenu.Items.AddSeparator();
        }

        public override void AddItem(string text, bool enabled, EventHandler onClick)
        {
            ArgChecker.AssertArgNotNullOrEmpty(text, "text");
            ArgChecker.AssertArgNotNull(onClick, "onClick");

            var item = new ButtonMenuItem { Text = text, Enabled = enabled };
            item.Click += (sender, e) => onClick(sender, e);
            _contextMenu.Items.Add(item);
        }

        public override void RemoveLastDivider()
        {
            if (_contextMenu.Items[_contextMenu.Items.Count - 1] is SeparatorMenuItem)
                _contextMenu.Items.RemoveAt(_contextMenu.Items.Count - 1);
        }

        public override void Show(RControl parent, RPoint location)
        {
            _contextMenu.Show(((ControlAdapter)parent).Control);
            //TODO: Eto 2.5 _contextMenu.Show(((ControlAdapter)parent).Control, Utils.ConvertRound(location));
        }

        public override void Dispose()
        {
            _contextMenu.Dispose();
        }
    }
}