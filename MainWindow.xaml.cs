using Microsoft.VisualBasic.FileIO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ETSYBUYER.ViewModels;
using SearchOption = System.IO.SearchOption;
using System.Text.RegularExpressions;
using log4net;

namespace ETSYBUYER
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly log4net.ILog log4
       = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
        {
            log4.Info("Start App");
            InitializeComponent();
            MainWindowViewModels vm = new MainWindowViewModels();
            vm.GenerateChromeProfile = new Commands.RelayCommand(Commands.Commands.GenerateChromeProfileCmd);
            vm.Run = new Commands.RelayCommand(Commands.Commands.Run);
            vm.ImportUser = new Commands.RelayCommand(Commands.Commands.ImportUser);
            vm.ImportKeyPair = new Commands.RelayCommand(Commands.Commands.ImportSearchText);
            this.DataContext = vm;
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(((TextBox)sender).Text + e.Text);
        }
        public static bool IsValid(string str)
        {
            double i;
            return double.TryParse(str, out i) && i >= 0 && i <= 100;
        }

        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(((TextBox)sender).Text + e.Text);
        }

        private void TextBox_PreviewTextInput_2(object sender, TextCompositionEventArgs e)
        {
            e.Handled= !IsValid(((TextBox)sender).Text + e.Text);

        }

    }
}