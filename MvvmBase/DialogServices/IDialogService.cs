using System.Windows;

namespace MvvmBase.DialogServices
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string content, string title, MessageBoxButton buttons);
        string SaveFileDialog(string fileName, string filter = null);
        void Show(ViewModelBase viewModel);
        bool? ShowDialog(ViewModelBase viewModel);

        void ShowProgressWindowAsync(string title, string message, bool isCancelable = false);
        void UpdateProgressWindow(double progressValue, string message);
        void CloseProgressWindow();

    }
}
