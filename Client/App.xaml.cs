using System.Windows;
using Client.ViewModels;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void OnStartUp(object sender, StartupEventArgs e)
        {
            ClientMainWindow view = new ClientMainWindow();

            Models.ClientModel clientModel = new Models.ClientModel();

            view.DataContext = new ClientViewModel(clientModel);

            view.Show();
        }
    }

}
