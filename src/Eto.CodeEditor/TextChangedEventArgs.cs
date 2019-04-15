using System;
namespace Eto.CodeEditor
{
    public enum TextChangeType {CharAdded, CharDeleted }

    public class TextChangedEventArgs : EventArgs
    {
        private TextChangeType textChangeType;
        private char charAddedOrDeleted;

        public TextChangedEventArgs(TextChangeType textChangeType, char charAddedOrDeleted)
        {
            this.textChangeType = textChangeType;
            this.charAddedOrDeleted = charAddedOrDeleted;
        }

        public TextChangeType TextChangeType => textChangeType;

        public char CharAddedOrDeleted => charAddedOrDeleted;
    }
}
