using System;
using System.Xml.Serialization;
using Client.ViewModels;
using MvvmBase;

namespace Client.Models
{
    [XmlInclude(typeof(Options))]
    public class Options : ViewModelBase
    {
        private string _userName = String.Empty;
        private string _ip = String.Empty;
        private string _port = String.Empty;

        public string UserName
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        public string Ip
        {
            get { return _ip; }
            set { Set(ref _ip, value); }
        }

        public string Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }
    }
}
