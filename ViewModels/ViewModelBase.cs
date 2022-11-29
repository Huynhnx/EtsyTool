using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETSYBUYER.ViewModels
{
    [Serializable]
    public class ViewModelBase : INotifyPropertyChanged
    {
        //basic ViewModelBase
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
