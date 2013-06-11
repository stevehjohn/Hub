using MingControls.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MingControls.Controllers
{
    public class TextEntryDialogController
    {
        private readonly TextEntryDialog _dialog;
        private readonly Window _owner;

        public TextEntryDialogController(Window owner, string title, string prompt)
        {
            _owner = owner;
            _dialog = new TextEntryDialog();
            _dialog.Title = title;
            _dialog.Prompt.Text = prompt;
        }

        public bool ShowDialog()
        {
            _dialog.Owner = _owner;
            _dialog.ShowDialog();
            return _dialog.Result;
        }

        public string Text
        {
            get 
            { 
                return _dialog.UserText.Text; 
            }
            set
            {
                _dialog.UserText.Text = value;
            }
        }
    }
}
