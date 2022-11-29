using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETSYBUYER.Commands;
using ETSYBUYER.Utils;

namespace ETSYBUYER.ViewModels
{
    public class MainWindowViewModels: ViewModelBase
    {
        private List<User> users = new List<User>();
        public List<User> Users
        {
            get
            {
                return users;
            }
            set
            {
                users = value;
                RaisePropertyChanged("Users");
            }
        }
        private List<SearchPair> searchPair = new List<SearchPair>();
        public List<SearchPair> SearchPair
        {
            get
            {
                return searchPair;
            }
            set
            {
                searchPair = value;
                RaisePropertyChanged("SearchPair");
            }
        }
        private int loopnumber;
        public int Loopnumber
        {
            get
            {
                return loopnumber;
            }
            set
            {
                loopnumber = value;
                RaisePropertyChanged("Loopnumber");
            }
        }
        public RelayCommand Run { get; set; }
        public RelayCommand ImportUser { get; set; }
        public RelayCommand ImportKeyPair { get; set; }
        public RelayCommand GenerateChromeProfile { get; set; }

    }
}
