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
            Client client = new Client();
            client.Connect("localhost", 50000);
            client.Connected += OnConnected;
            ClientMainWindow view = new ClientMainWindow();
            //view.DataContext = new UserMessagesViewModel();
            view.Show();
        }

        void OnConnected(Client client)
        {
            client.WriteMessageAsync(new MessageData() {Message = "Hello!"});
        }
    }

}
