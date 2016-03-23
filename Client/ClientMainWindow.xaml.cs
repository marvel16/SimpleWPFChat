using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for ClientMainWindow.xaml
    /// </summary>
    public partial class ClientMainWindow : MetroWindow
    {
        public ClientMainWindow()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)MessageBox.Items).CollectionChanged += ListView_CollectionChanged;
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // scroll the new item into view   
                MessageBox.ScrollIntoView(e.NewItems[0]);
            }
        }

        private void InputTextbox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var txtbox = sender as TextBox;
            if (txtbox == null)
                return;

            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                txtbox.Text += Environment.NewLine;
                txtbox.CaretIndex = txtbox.Text.Length - 1;
            }
        }

        private void InputTextbox_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.


            }
        }

        private void InputTextbox_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }


    }
}
