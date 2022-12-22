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
        private List<User> selecteduser = new List<User>();
        public List<User> SelectedUser
        {
            get
            {
                return selecteduser;
            }
            set
            {
                selecteduser = value;
                RaisePropertyChanged("SelectedUser");
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
        private int timeonpage = 20;
        public int TimeOnPage
        {
            get
            {
                return timeonpage;
            }
            set
            {
                timeonpage = value;
                RaisePropertyChanged("TimeOnPage");
            }
        }
        private int searchpages = 2;
        public int SearchPages
        {
            get
            {
                return searchpages;
            }
            set
            {
                searchpages = value;
                RaisePropertyChanged("SearchPages");
            }
        }
        private double favoriterate;
        public double FavoriteRate
        {
            get
            {
                return favoriterate;
            }
            set
            {
                favoriterate = value;
                RaisePropertyChanged("FavoriteRate");
            }
        }
        private double chatrate;
        public double ChatRate
        {
            get
            {
                return chatrate;
            }
            set
            {
                chatrate = value;
                RaisePropertyChanged("ChatRate");
            }
        }
        public RelayCommand Run { get; set; }
        public RelayCommand ImportUser { get; set; }
        public RelayCommand ImportKeyPair { get; set; }
        public RelayCommand GenerateChromeProfile { get; set; }

    }
}
