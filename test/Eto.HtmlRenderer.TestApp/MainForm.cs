using System;
using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Eto;
using Eto.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Eto.HtmlRenderer.TestApp
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Title = "Eto.HtmlRenderer Test App";
            ClientSize = new Size(400, 400);
            if (Platform.IsMac)
                Menu = new MenuBar();

            var htmlPanel = new HtmlPanel();
            htmlPanel.CanFocus = true;
            htmlPanel.IsSelectionEnabled = true;
            //htmlPanel.MaximumSize = new Size(300, int.MaxValue);
            htmlPanel.ImageLoad += (sender, e) =>
            {
                e.Callback(Bitmap.FromResource($"Eto.HtmlRenderer.TestApp.{e.Src}"));
            };
            htmlPanel.Text = "<p>This is some <b>bold</b> text.</p><img src='Logo.png'>";
            Content = new Scrollable { Content = htmlPanel, ExpandContentWidth = false, ExpandContentHeight = false };
        }
    }
}
