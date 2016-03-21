using System;
using System.Windows;
using System.Windows.Forms;

namespace MvvmBase.DialogServices
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string content, string title, MessageBoxButton buttons);

        string SaveFileDialog(string fileName, string filter = null);
        void Show(ViewModelBase viewModel);

        bool? ShowDialog(ViewModelBase viewModel);
    }
}
