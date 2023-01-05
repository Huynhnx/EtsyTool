using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ETSYBUYER.ViewModels;
using ETSYBUYER.Utils;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Windows.Media.Media3D;
using System.Windows;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Windows.Data;

namespace ETSYBUYER.Commands
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            try
            {
                if (timeoutInSeconds > 0)
                {

                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    IWebElement el = wait.Until(drv => drv.FindElement(by));
                    return el;
                }



                return driver.FindElement(by);
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
        public static void ScrollToRandom(IWebDriver driver, int timeonpage)
        {
            MainWindow.log4.Info("ScrollToRandom");
            try
            {
                IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;
                var hight = (long)js1.ExecuteScript("return document.body.scrollHeight;");
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                long newScrollHeight = 0;
                do
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    // Random random = new Random();
                    // newScrollHeight= (long)random.Next(1,(int)hight);
                    newScrollHeight += hight / 10;
                    Thread.Sleep((timeonpage * 1000) / 10);
                    js.ExecuteScript("window.scrollTo(0,'" + newScrollHeight + "');");

                    if (stopWatch.Elapsed.TotalSeconds >= timeonpage)
                    {
                        break;
                    }

                } while (true);
                stopWatch.Stop();
            }
            catch (System.Exception ex)
            {
                MainWindow.log4.Error(ex.Message);
            }
           
        }
       
    }
    class ExcelHelper
    {
        /// <summary>
        /// Get string value of an Excel Cell.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = string.Empty;
            if (cell.CellValue != null)
            {
                value = cell.CellValue.InnerXml.ToString();
            }
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[int.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
        public static bool IsFileLocked(string filepath)
        {
            FileInfo file = new FileInfo(filepath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
    public class Commands
    {
        static string ProfileFolder = "C:/Users/" + System.Environment.GetEnvironmentVariable("USERNAME") + "/AppData/Local/Google/Chrome/User Data";
        public static bool Login(User use, ChromeDriver driver)
        {
            //Click to sign in
            try
            {
                var signin = driver.FindElement(By.XPath("//*[@id=\"gnav-header-inner\"]/div[4]/nav/ul/li[1]/button"));
                signin.Click();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                //put user/pass then sigin
                var email = driver.FindElement(By.XPath("//*[@id=\"join_neu_email_field\"]"));
                email.SendKeys(use.UserName);
                Thread.Sleep(1000);
                var password = driver.FindElement(By.XPath("//*[@id=\"join_neu_password_field\"]"));
                password.SendKeys(use.Password);
                Thread.Sleep(1000);
                var submit = driver.FindElement(By.XPath("//*[@id=\"join-neu-form\"]/div[1]/div/div[7]/div/button"));
                submit.Click();
                Thread.Sleep(2000);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                return true;
            }
            catch (System.Exception ex)
            {

            }
            return false;
        }
        public static List<User> ImportUserFromExcel()
        {
            List<User> Output = new List<User>();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog.RestoreDirectory = false;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string outputFileName = openFileDialog.FileName;
                // Check file exist or not
                if (ExcelHelper.IsFileLocked(outputFileName))
                {
                    MessageBox.Show("This File is Lock", "Warnning");
                    return Output;
                }
                try
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(outputFileName, false))
                    {
                        // Get the SharedStringTablePart. If it does not exist, create a new one.
                        WorkbookPart workbookPart = document.WorkbookPart;
                        IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                        string relationshipId = sheets.First().Id.Value;
                        WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                        Worksheet workSheet = worksheetPart.Worksheet;
                        SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                        // Get rows data from the sheet
                        IEnumerable<Row> rows = sheetData.Descendants<Row>();
                        // We only accept excel file that has 2 columns (layer name, color and transparency)
                        if (rows.ElementAt(0).Descendants<Cell>().Count() != 1)
                        {
                            MessageBox.Show("File Format is wrong. File should have 3 column", "Warning");
                            return Output;
                        }

                        foreach (Row row in rows)
                        {
                            // Skip header row
                            //if (row == rows.ElementAt(0))
                            //    continue;
                            if (ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(0)) == string.Empty)
                            {
                                continue;
                            }

                            // Get cell value and paste into new Layer Item
                            string profilepath = ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(0));
                            // Skip invalid name
                            if (profilepath == string.Empty)
                            {
                                continue;
                            }
                            
                            User user = new User();
                            user.ProfilePath = profilepath;
                            Output.Add(user);
                        }

                    }
                }
                // Using OpenXml to read the excel file
                catch (System.Exception exc)
                {
                    return Output;
                }
                return Output;
            }
            return null;
        }
        public static void GenerateChromeProfileCmd(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                for (int i = 0; i < vm.Users.Count(); i++)
                {
                    try
                    {
                        //string ip = "";
                        //string username = "mix101CUHDZ8C";
                        //string pass = "diGifK30";
                        var dir = AppDomain.CurrentDomain.BaseDirectory;
                        string chromeDriverPath = dir + "ChromeDriver";
                        var options = new ChromeOptions();
                        
                        //if (!string.IsNullOrEmpty(ip))
                        //{
                        //    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(pass))
                        //    {
                        //        options.AddExtension("ProxyAuth.crx");
                        //    }
                        //    options.AddArgument(string.Format("--proxy-server={0}", ip));
                        //}

                        if (!Directory.Exists(ProfileFolder))
                        {
                            Directory.CreateDirectory(ProfileFolder);
                        }

                        if (Directory.Exists(ProfileFolder))
                        {
                            options.AddArguments("user-data-dir=" + ProfileFolder + "/" + vm.Users[i].UserName);
                        }
                        var driver = new ChromeDriver(chromeDriverPath,options);
                        //if (!string.IsNullOrEmpty(ip))
                        //{
                        //    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
                        //    {
                        //        driver.Url = "chrome-extension://ggmdpepbjljkkkdaklfihhngmmgmpggp/options.html";
                        //        driver.Navigate();

                        //        driver.FindElement(By.Id("login")).SendKeys(username);
                        //        driver.FindElement(By.Id("password")).SendKeys(pass);
                        //        driver.FindElement(By.Id("retry")).Clear();
                        //        driver.FindElement(By.Id("retry")).SendKeys("2");

                        //        driver.FindElement(By.Id("save")).Click();
                        //    }
                        //}
                        string url = "https://www.whoer.net/";
                        driver.Url = url;
                        driver.Navigate().GoToUrl(url);
                        driver.Quit();
                        string lnkFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), vm.Users[i].UserName+".lnk");
                        Shortcut.Create(lnkFileName,
                            @"C:\Program Files\Google\Chrome\Application\"+"chrome.exe",
                            "--profile-directory="+ vm.Users[i].UserName, null, "Chrome Profile", "Ctrl+Shift+N", null);
                        //if (!System.IO.Directory.Exists(ProfileFolder + "/" + vm.Users[i].UserName))
                        //{
                        //    var dir = AppDomain.CurrentDomain.BaseDirectory;
                        //    string chromeDriverPath = dir + "ChromeDriver";
                        //    var options = new ChromeOptions();
                        //    options.AddArgument("no-sandbox");
                        //    options.AddArgument("user-data-dir=D:\\ChromeDriver\\scoped_dir13972_618222406");
                        //    options.AddArgument("headless");
                        //    var driver = new ChromeDriver(chromeDriverPath, options, TimeSpan.FromDays(20));
                        //    var profile = (IDictionary<string, object>)driver.Capabilities["chrome"];
                        //    object x;
                        //    profile.TryGetValue("userDataDir", out x);
                        //    string tmp = x.ToString();
                        //    if (System.IO.Directory.Exists(tmp))
                        //    {
                        //        string url = "https://www.etsy.com/";
                        //        driver.Url = url;
                        //        //Login(vm.Users[i], driver);
                        //        var folderDes = System.IO.Directory.CreateDirectory(ProfileFolder + "/" + vm.Users[i].UserName);
                        //        CopyFilesRecursively(tmp, folderDes.FullName);
                        //        driver.Quit();
                        //    }
                        //}
                    }
                    catch(System.Exception exx)
                    {

                    }
                    

                }
            }

        }
        public static bool IsLogin(ChromeDriver driver)
        {
            try
            {
                MainWindow.log4.Info("Double-check for login!!!");
                var signin = driver.FindElement(By.XPath("//*[@id=\"gnav-header-inner\"]/div[4]/nav/ul/li[1]/button"), 5);
                if (signin == null)
                {
                    MainWindow.log4.Info("Already login");
                    return true;
                }
                MainWindow.log4.Info("Not Login yet");
                return false;
            }
            catch(System.Exception ex)
            {
                MainWindow.log4.Error(ex);
            }
            
            return true;
        }
        public static bool IsAdvertiseItem(ChromeDriver driver)
        {
            MainWindow.log4.Info("IsAdvertiseItem?");
            IWebElement ad = null;
            try
            {
                ad = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"content\"]/div/div[1]/div/div[4]/div[11]/div[2]/div[10]/div[1]/div/div/ul/li[1]/div/div/a/div[2]/p[2]/span[2]"), 10);
            }
            catch (System.Exception ex)
            {

            }
            if (ad == null)
            {
                MainWindow.log4.Info("Not An Advertise?");
                return false;
            }
            return true;
        }
        public static void Run(object obj)
        {
            string log = "";
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                for (int i = 0; i < vm.Loopnumber; i++)
                {
                    foreach (var key in vm.SearchPair)
                    {
                        try
                        {
                            Random rand = new Random();
                            int r = rand.Next(vm.Users.Count);
                            User u = vm.Users[r];
                            log += "Ran profile: " + u.ProfilePath;
                            MainWindow.log4.Info("Ran profile: " + u.ProfilePath);
                            var chromeDriverPath = AppDomain.CurrentDomain.BaseDirectory;
                            var username = System.Environment.GetEnvironmentVariable("USERNAME");
                            var ProfileFolder = "C:/Users/" + username + "/AppData/Local/Google/Chrome/User Data";
                            var options = new ChromeOptions();
                            options.AddArgument("user-data-dir="+ ProfileFolder);
                            options.AddArgument("profile-directory="+ u.ProfilePath);
                            //options.AddArgument("headless");
                            MainWindow.log4.Info("Create Chrome Driver Instance");
                            var driver = new ChromeDriver(chromeDriverPath,options);
                            MainWindow.log4.Info("Done!!!");
                            string url = "https://www.etsy.com/";
                            driver.Url = url;
                            MainWindow.log4.Info("Navigate to Etsy.com");
                            driver.Navigate().GoToUrl(url);
                            //Chat rate
                            var chatrand = new Random();
                            var chatrandcollection = vm.Users.OrderBy(x => rand.Next(vm.Users.Count))
                                                           .Take((int)((float)(vm.ChatRate/100) * vm.Users.Count)).ToList();

                            //favorite
                            var favoriterand = new Random();
                            var favoriteCollection = vm.Users.OrderBy(x => rand.Next(vm.Users.Count))
                                                      .Take((int)((float)(vm.FavoriteRate / 100) * vm.Users.Count)).ToList();
                            if (IsLogin(driver))
                            {
                                //search item
                                MainWindow.log4.Info("Send Search Key:"+ key.SearchKey);
                                var search = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"global-enhancements-search-query\"]"), 20);
                                search.SendKeys(key.SearchKey);
                                Thread.Sleep(1000);
                                var searchbtn = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"gnav-search\"]/div/div[1]/button[2]"), 100);
                                searchbtn.Click();
                                Thread.Sleep(1000);
                               
                                bool bFound = false;
                                int count = 0;
                                for (int j=0;j<vm.SearchPages;j++)
                                {
                                    MainWindow.log4.Info("Search in the page number "+(j+1).ToString());
                                    //var list = driver.FindElements(By.TagName("a"));
                                    var x = driver.FindElement(By.CssSelector("#content > div > div.wt-bg-white.wt-grid__item-md-12.wt-pl-xs-1.wt-pr-xs-0.wt-pr-md-1.wt-pl-lg-0.wt-pr-lg-0.wt-bb-xs-1 > div > div.wt-mt-xs-3.wt-text-black > div.wt-grid.wt-pl-xs-0.wt-pr-xs-0.search-listings-group > div:nth-child(2) > div.wt-bg-white.wt-display-block.wt-pb-xs-2.wt-mt-xs-0 > div:nth-child(1) > div > div"));
                                    MainWindow.log4.Info("Find element in grid result");
                                    var list= x.FindElements(By.TagName("a"));
                                    foreach (WebElement el in list)
                                    {
                                        if (el == null)
                                        {
                                            continue;
                                        }
                                        String link = el.GetAttribute("href");
                                        if (link != null && link.Contains(key.Id))
                                        {
                                            //is not ad
                                            MainWindow.log4.Info("Found the Item");
                                            if (IsAdvertiseItem(driver) == false)
                                            {
                                                log += "Found Item" +"Keyword Search:" + key.SearchKey + " Listing ID:" + key.Id + "On Page "+(j+1).ToString()+"\n";
                                                bFound = true;
                                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                                driver.Navigate().GoToUrl(link);
                                                MainWindow.log4.Info("Navigate to Item");
                                                Random time = new Random();
                                                int timeonpage = time.Next(vm.TimeOnPageFrom, vm.TimeOnPageTo);
                                                log += "Time On Page: " + timeonpage.ToString() ;
                                                WebDriverExtensions.ScrollToRandom(driver, timeonpage);
                                                if (favoriteCollection.Contains(u))
                                                {
                                                    MainWindow.log4.Info("Add favourite");
                                                    try
                                                    {
                                                        var addfavorite = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"listing-right-column\"]/div/div[1]/div[1]/div/div/div[2]/button"), 100);
                                                        log += "Favourite: Yes";
                                                        addfavorite.Click();
                                                    }
                                                    catch(System.Exception ex)
                                                    {
                                                        MainWindow.log4.Error(ex.Message);
                                                    }
                                                   
                                                }
                                                else
                                                {
                                                    log += "Favourite: No";
                                                }

                                                if (chatrandcollection.Contains(u))
                                                {
                                                    MainWindow.log4.Info("Chat");
                                                    try
                                                    {
                                                        var Chat = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"desktop_shop_owners_parent\"]/div/div/a"), 100);
                                                        Chat.Click();

                                                        var message = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"chat-ui-composer\"]/div[1]/div[1]/textarea"), 100);
                                                        var chattext = ChatText();
                                                        Random randchat = new Random();
                                                        int r2 = randchat.Next(chattext.Count);
                                                        message.SendKeys(chattext[r2]);
                                                        var sentmessage = WebDriverExtensions.FindElement(driver, By.XPath("//*[@id=\"chat-ui-composer\"]/div[1]/div[2]/button"), 100);
                                                        sentmessage.Click();
                                                        MainWindow.log4.Info("Send message:" + chattext[r2]);
                                                        Thread.Sleep(2000);
                                                        log += "Chat:" + chattext[r2];
                                                    }
                                                    catch (System.Exception ex)
                                                    {
                                                        MainWindow.log4.Error(ex.Message);
                                                    }
                                                    
                                                }

                                                break;
                                            }
                                        }
                                       
                                    }
                                    if (bFound)
                                    {
                                        break;
                                    }
                                    if (!bFound && count < (vm.SearchPages-1))
                                    {
                                        var xpath = "/html/body/main/div/div[1]/div/div[3]/div[8]/div[2]/div[13]/div/div/div/div[2]/nav/ul/li[" + (count + 3).ToString() + "]/a";
                                        //var n = driver.FindElement(By.XPath(xpath));
                                        //Thread.Sleep(1000);
                                        try
                                        {
                                            MainWindow.log4.Info("Navigate to next page");
                                            var nextpage = WebDriverExtensions.FindElement(driver, By.XPath(xpath), 10);
                                            if (nextpage != null)
                                            {
                                                nextpage.Click();
                                                Thread.Sleep(2000);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        catch (System.Exception ex)
                                        {
                                            
                                            MainWindow.log4.Error(ex.Message);
                                            break;
                                        }
                                       
                                        
                                    }
                                    else
                                    {
                                        log += "Not Found Keyword Search:" + key.SearchKey + " Listing ID:" + key.Id ;
                                    }
                                    
                                    count++;
                                }
                               
                            }
                            else
                            {
                                log += "The profile " + u.UserName + " is not login.Please login manual first";
                            }
                            driver.Quit();
                            
                        }
                        catch (SystemException exx)
                        {

                        }

                    }

                }
                vm.LogText = log;
            }
        }
        public static void ImportUser(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                List<User> users = ImportUserFromExcel();
                vm.Users = users;

            }
        }
        public static void ImportSearchText(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                List<SearchPair> listdic = new List<SearchPair>();
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog.RestoreDirectory = false;
                openFileDialog.CheckFileExists = true;
                if (openFileDialog.ShowDialog() == true)
                {
                    string outputFileName = openFileDialog.FileName;
                    // Check file exist or not
                    if (ExcelHelper.IsFileLocked(outputFileName))
                    {
                        MessageBox.Show("This File is Lock", "Warnning");
                        return;
                    }
                    try
                    {
                        using (SpreadsheetDocument document = SpreadsheetDocument.Open(outputFileName, false))
                        {
                            // Get the SharedStringTablePart. If it does not exist, create a new one.
                            WorkbookPart workbookPart = document.WorkbookPart;
                            IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                            string relationshipId = sheets.First().Id.Value;
                            WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                            Worksheet workSheet = worksheetPart.Worksheet;
                            SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                            // Get rows data from the sheet
                            IEnumerable<Row> rows = sheetData.Descendants<Row>();
                            // We only accept excel file that has 2 columns (layer name, color and transparency)
                            if (rows.ElementAt(0).Descendants<Cell>().Count() != 2)
                            {
                                MessageBox.Show("File Format is wrong. File should have 3 column", "Warning");
                                return;
                            }

                            foreach (Row row in rows)
                            {
                                // Skip header row
                                //if (row == rows.ElementAt(0))
                                //    continue;
                                if (ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(0)) == string.Empty)
                                {
                                    continue;
                                }

                                // Get cell value and paste into new Layer Item
                                string Key = ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(0));
                                // Skip invalid name
                                if (Key == string.Empty)
                                {
                                    continue;
                                }
                                string ListingID = ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(1));
                                if (ListingID == string.Empty)
                                {
                                    continue;
                                }
                                SearchPair text = new SearchPair();
                                text.SearchKey = Key;
                                text.Id = ListingID;
                                listdic.Add(text);
                            }

                        }
                    }
                    // Using OpenXml to read the excel file
                    catch (System.Exception exc)
                    {
                    }
                }
                if (listdic.Count > 0)
                {
                    vm.SearchPair = listdic;
                }

            }
        }
        public static void LoginManual(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                foreach (var user in vm.SelectedUser)
                {
                    try
                    {
                        var chromeDriverPath = AppDomain.CurrentDomain.BaseDirectory;
                        var options = new ChromeOptions();
                        options.AddArgument("no-sandbox");
                        options.AddArgument("user-data-dir=" + ProfileFolder + "/" + user.UserName);

                        //options.AddArgument("headless");
                        var driver = new ChromeDriver(chromeDriverPath, options);

                        string url = "https://www.etsy.com/";
                        driver.Url = url;

                        driver.Navigate().GoToUrl(url);
                        var signin = driver.FindElement(By.XPath("//*[@id=\"gnav-header-inner\"]/div[4]/nav/ul/li[1]/button"));
                        signin.Click();
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    }
                    catch { }
                    
                }
            }
        }
        public static List<string> ChatText()
        {
            List<string> chattext = new List<string>();
            chattext= File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/ChatText.txt").ToList();
            return chattext;
        }
    }
}
