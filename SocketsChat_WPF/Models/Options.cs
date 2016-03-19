using System;
using System.Xml.Serialization;
using Client.ViewModels;

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
            set
            {
                if (_userName == value)
                    return;
                _userName = value;
                OnPropertyChanged();
            }
        }

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
