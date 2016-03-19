using System;
using System.Windows.Controls;

namespace Client.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Client.Commands.RelayCommand" />
    public class TextBoxEnterNewLineCommand : RelayCommand
    {
        public TextBoxEnterNewLineCommand() : base(o =>
            {
                var txtBox = o as TextBox;
                if (txtBox == null)
                    return;
                txtBox.Text += Environment.NewLine;
                txtBox.CaretIndex = txtBox.Text.Length - 1;
            }) {}
    }

    }
