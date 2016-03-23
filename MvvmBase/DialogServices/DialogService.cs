using System;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MvvmBase.Commands;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace MvvmBase.DialogServices
{
    public class DialogService : IDialogService
    {
        private static IDialogService instance = new DialogService();

        ProgressDialogController controller { get; set; }

        public static IDialogService Instance
        {
            get { return instance; }
        }

        public string SaveFileDialog(string fileName, string filter)
        {
            var dialog = new SaveFileDialog
            {
                Filter = string.IsNullOrEmpty(filter) ? "All files (*.*)|*.*" : filter,
                FileName = fileName,
                Title = "Save file dialog",
                ValidateNames = true,
            };

            dialog.ShowDialog();

            return dialog.FileName;
        }

        public bool? ShowDialog(ViewModelBase viewModel)
        {
            var dialogView = new DialogView
            {
                Owner = Application.Current.MainWindow,
                Title = viewModel.Title,
                DataContext = viewModel,
                ShowInTaskbar = false,
            };
            viewModel.CloseCmd = new RelayCommand(dialogView.Close);
            return dialogView.ShowDialog();
        }

        public void Show(ViewModelBase viewModel)
        {
            var dialogView = new DialogView
            {
                DataContext = viewModel,
                Title = viewModel.Title
            };

            viewModel.CloseCmd = new RelayCommand(dialogView.Close);
            dialogView.Loaded += viewModel.OnLoaded;
            dialogView.Show();
        }

        public MessageBoxResult ShowMessageBox(string content, string title, MessageBoxButton buttons)
        {
            return MessageBox.Show(content, title, buttons);
        }


        public void ShowProgressWindowAsync(string title, string message, bool isCancelable = false)
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                MetroWindow mainWindow = (MetroWindow)Application.Current.MainWindow;
                controller = await mainWindow.ShowProgressAsync(title, message, isCancelable);
            });
        }

        public void UpdateProgressWindow(double progressValue, string message)
        {
            if (controller != null)
            {
                controller.SetProgress(progressValue);
                controller.SetMessage(message);
            }
        }

        public async void CloseProgressWindow()
        {
            if (controller != null)
            {
                await controller.CloseAsync();
            }
        }
    }
}
