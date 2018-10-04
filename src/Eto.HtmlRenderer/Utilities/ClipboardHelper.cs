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
using System.Text;
using Eto.Forms;

namespace TheArtOfDev.HtmlRenderer.Eto.Utilities
{
    /// <summary>
    /// Helper to encode and set HTML fragment to clipboard.<br/>
    /// See http://theartofdev.wordpress.com/2012/11/11/setting-html-and-plain-text-formatting-to-clipboard/.<br/>
    /// <seealso cref="CreateDataObject"/>.
    /// </summary>
    /// <remarks>
    /// The MIT License (MIT) Copyright (c) 2014 Arthur Teplitzki.
    /// </remarks>
    internal static class ClipboardHelper
    {
        /// <summary>
        /// Create <see cref="DataObject"/> with given html and plain-text ready to be used for clipboard or drag and drop.<br/>
        /// Handle missing <![CDATA[<html>]]> tags, specified start\end segments and Unicode characters.
        /// </summary>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static DataObject CreateDataObject(string html, string plainText)
        {
            return new DataObject
            {
                Html = html,
                Text = plainText
            };
        }

        /// <summary>
        /// Clears clipboard and sets the given HTML and plain text fragment to the clipboard, providing additional meta-information for HTML.<br/>
        /// See <see cref="CreateDataObject"/> for HTML fragment details.<br/>
        /// </summary>
        /// <example>
        /// ClipboardHelper.CopyToClipboard("Hello <b>World</b>", "Hello World");
        /// </example>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string html, string plainText)
        {
            var dataObject = new Clipboard
            {
                Html = html,
                Text = plainText
            };
        }

        /// <summary>
        /// Clears clipboard and sets the given plain text fragment to the clipboard.<br/>
        /// </summary>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string plainText)
        {
            var dataObject = new Clipboard();
            dataObject.Text = plainText;
        }

    }
}