using System;
using System.Collections.Generic;

namespace Eto.CodeEditor
{
    public enum TextChangeType {CharAdded, CharDeleted, Modified }

    public class TextChangedEventArgs : EventArgs
    {
        private TextChangeType textChangeType;
        private char charAddedOrDeleted;

        public TextChangedEventArgs(TextChangeType textChangeType, char charAddedOrDeleted)
        {
            this.textChangeType = textChangeType;
            this.charAddedOrDeleted = charAddedOrDeleted;
        }

        public bool NewLineAdded => charAddedOrDeleted == '\n' || charAddedOrDeleted == '\r';

        public TextChangeType TextChangeType => textChangeType;

        public char CharAddedOrDeleted => charAddedOrDeleted;
    }
}
