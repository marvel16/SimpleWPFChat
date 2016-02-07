using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CustomNetworkExtensions;

namespace SocketsChat_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void OnStartUp(object sender, StartupEventArgs e)
        {
            ClientMainWindow view = new ClientMainWindow();

            Client client = new Client();

            view.DataContext = new ClientViewModel(client);

            view.Show();
        }
    }

}
