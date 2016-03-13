using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SocketsChat_WPF
{
    [XmlInclude(typeof(Options))]
    public class Options : ViewModelBase
    {
        private string _userName = String.Empty;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName == value)
                    return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        private string _ip = String.Empty;
        public string Ip
        {
            get { return _ip; }
            set
            {
                if (_ip == value)
                    return;
                _ip = value;
                OnPropertyChanged();
            }
        }

        private string _port = String.Empty;
        public string Port
        {
            get { return _port; }
            set
            {
                if (_port == value)
                    return;
                _port = value;
                OnPropertyChanged();
            }
        }
    }

    
}
