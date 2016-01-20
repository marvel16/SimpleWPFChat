using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SocketsChat_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void OnStartUp(object sender, StartupEventArgs e)
        {
            var msgs = new List<UserMessageViewModel>
            {
                new UserMessageViewModel() {Message = "ASDSAD SAD A123213213213213213D ASD SAD ASD ADS "},
                new UserMessageViewModel() {Message = "ASDSADdssdsadasdasdD ASD ADS SAD ASD SAD ASD ADS "},
                new UserMessageViewModel() {Message = "ASDSbdbdbdb basbdbdbd ASD ASD ADS SAD ASD SAD ASD ADS "},
            };

            ClientMainWindow view = new ClientMainWindow();
            view.DataContext = new UserMessagesViewModel(msgs);
            view.Show();
        }
    }

}
