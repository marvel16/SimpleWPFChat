using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Serialization;
using MvvmBase.Properties;

namespace MvvmBase
{
    
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        [XmlIgnore]
        public ICommand CloseCmd { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public string Title { get; protected set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnLoaded(object sender, EventArgs e)
        {
            
        }

        public virtual void PerformClosing()
        {
            CloseCmd?.Execute(null);
        }

        protected bool Set<T>(ref T field, T propertyValue, [CallerMemberName] string propertyName = null, bool validateProperty = true)
        {
            if (EqualityComparer<T>.Default.Equals(field, propertyValue))
            {
                return false;
            }

            field = propertyValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (validateProperty)
            {
                //this.ValidateProperty(propertyValue, true, propertyName);
            }
            return true;
        }

    }
}
