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

namespace ETSYBUYER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModels vm = new MainWindowViewModels();
            vm.GenerateChromeProfile = new Commands.RelayCommand(Commands.Commands.GenerateChromeProfileCmd);
            vm.Run = new Commands.RelayCommand(Commands.Commands.Run);
            vm.ImportUser = new Commands.RelayCommand(Commands.Commands.ImportUser);
            vm.ImportKeyPair = new Commands.RelayCommand(Commands.Commands.ImportSearchText);
            this.DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string chromeDriverPath = @"D:\ChromeDriver";
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            options.AddArgument("user-data-dir=D:\\ChromeDriver\\scoped_dir13972_618222406");
            //options.AddArgument("headless");
            var driver = new ChromeDriver(chromeDriverPath, options, TimeSpan.FromDays(20));
            
            string url = "https://www.etsy.com/";
            driver.Url = url;

            driver.Navigate().GoToUrl(url);
            //Click to sign in
            try
            {
                var signin = driver.FindElement(By.XPath("//*[@id=\"gnav-header-inner\"]/div[4]/nav/ul/li[1]/button"));
                signin.Click();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                //put user/pass then sigin
                var email = driver.FindElement(By.XPath("//*[@id=\"join_neu_email_field\"]"));
                email.SendKeys("huynhnx93@gmail.com");
                Thread.Sleep(1000);
                var password = driver.FindElement(By.XPath("//*[@id=\"join_neu_password_field\"]"));
                password.SendKeys("Nguyenhuynh1993");
                Thread.Sleep(1000);
                var submit = driver.FindElement(By.XPath("//*[@id=\"join-neu-form\"]/div[1]/div/div[7]/div/button"));
                submit.Click();
                Thread.Sleep(2000);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            }
            catch (System.Exception ex)
            {

            }

            var search = driver.FindElement(By.XPath("//*[@id=\"global-enhancements-search-query\"]"));
            search.SendKeys("200k");
            Thread.Sleep(1000);
            var searchbtn = driver.FindElement(By.XPath("//*[@id=\"gnav-search\"]/div/div[1]/button[2]"));
            searchbtn.Click();
            Thread.Sleep(1000);
            var list = driver.FindElements(By.TagName("a"));
            bool bFound = false;
            foreach (WebElement el in list)
            {
                String link = el.GetAttribute("href");
                if (link!= null && link.Contains("1230788601"))
                {
                    var ad = driver.FindElement(By.XPath("//*[@id=\"content\"]/div/div[1]/div/div[4]/div[11]/div[2]/div[10]/div[1]/div/div/ul/li[1]/div/div/a/div[2]/p[2]/span[2]"));
                    if ( ad == null)
                    {
                        bFound = true;
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        driver.Navigate().GoToUrl(link);
                        var element = driver.FindElement(By.XPath("//*[@id=\"collage-footer\"]/footer/div/div[2]/div/div/div[2]"));
                        Actions actions = new Actions(driver);
                        Thread.Sleep(1000);
                        actions.MoveToElement(element).Perform();
                        break;
                    }
                }

            }
            if (!bFound)
            {
                MessageBox.Show("Can not found any items in the search list");
            }
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var username = System.Environment.GetEnvironmentVariable("USERNAME");
            var ProfileFolder = "C:/Users/" + username + "/AppData/Local/Google/Chrome/User Data";
            string chromeDriverPath = @"D:\ChromeDriver";
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            //options.AddArgument("user-data-dir=D:\\ChromeDriver\\scoped_dir13972_618222406");
            options.AddArgument("headless");
            var driver = new ChromeDriver(chromeDriverPath, options, TimeSpan.FromDays(20));
            var profile = (IDictionary<string, object>)driver.Capabilities["chrome"];
            object x;
            profile.TryGetValue("userDataDir",out x);
            string tmp = x.ToString();
            //if (System.IO.Directory.Exists(tmp))
            //{
            //    var folderDes= System.IO.Directory.CreateDirectory(ProfileFolder + "/Abc");
            //    CopyFilesRecursively(tmp, folderDes.FullName);
            //}
        }
    }
}