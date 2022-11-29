using Microsoft.Win32;
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
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;

namespace ETSYBUYER.Commands
{
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
                        if (rows.ElementAt(0).Descendants<Cell>().Count() != 2)
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
                            string User = ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(0));
                            // Skip invalid name
                            if (User == string.Empty)
                            {
                                continue;
                            }
                            string Pass = ExcelHelper.GetCellValue(document, row.Descendants<Cell>().ElementAt(1));
                            if (Pass == string.Empty)
                            {
                                continue;
                            }
                            User user = new User();
                            user.UserName = User;
                            user.Password = Pass;
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
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
        public static void GenerateChromeProfileCmd(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                for (int i = 0; i < vm.Users.Count(); i++)
                {
                    var username = System.Environment.GetEnvironmentVariable("USERNAME");
                    var ProfileFolder = "C:/Users/" + username + "/AppData/Local/Google/Chrome/User Data";
                    if (!System.IO.Directory.Exists(ProfileFolder + "/" + vm.Users[i].UserName))
                    {
                        string chromeDriverPath = @"D:\ChromeDriver";
                        var options = new ChromeOptions();
                        options.AddArgument("no-sandbox");
                        //options.AddArgument("user-data-dir=D:\\ChromeDriver\\scoped_dir13972_618222406");
                        options.AddArgument("headless");
                        var driver = new ChromeDriver(chromeDriverPath, options, TimeSpan.FromDays(20));
                        var profile = (IDictionary<string, object>)driver.Capabilities["chrome"];
                        object x;
                        profile.TryGetValue("userDataDir", out x);
                        string tmp = x.ToString();
                        if (System.IO.Directory.Exists(tmp))
                        {
                            string url = "https://www.etsy.com/";
                            driver.Url = url;
                            //Login(vm.Users[i], driver);
                            var folderDes = System.IO.Directory.CreateDirectory(ProfileFolder + "/" + vm.Users[i].UserName);
                            CopyFilesRecursively(tmp, folderDes.FullName);
                            driver.Quit();
                        }
                    }

                }
            }

        }
        public static void Run(object obj)
        {
            MainWindow v = obj as MainWindow;
            if (v != null)
            {
                MainWindowViewModels vm = v.DataContext as MainWindowViewModels;
                for (int i = 0; i < vm.Loopnumber; i++)
                {
                    Random rand = new Random();
                    int r = rand.Next(vm.Users.Count);
                    User u = vm.Users[r];
                    string chromeDriverPath = @"D:\ChromeDriver";
                    var username = System.Environment.GetEnvironmentVariable("USERNAME");
                    var ProfileFolder = "C:/Users/" + username + "/AppData/Local/Google/Chrome/User Data";
                    var options = new ChromeOptions();
                    options.AddArgument("no-sandbox");
                    options.AddArgument("user-data-dir=" + ProfileFolder + "/" + u.UserName);
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
                        if (link != null && link.Contains("1230788601"))
                        {
                            IWebElement ad = null;
                            try
                            {
                                ad = driver.FindElement(By.XPath("//*[@id=\"content\"]/div/div[1]/div/div[4]/div[11]/div[2]/div[10]/div[1]/div/div/ul/li[1]/div/div/a/div[2]/p[2]/span[2]"));
                            }
                            catch (System.Exception ex)
                            {

                            }

                            if (ad == null)
                            {
                                bFound = true;
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                driver.Navigate().GoToUrl(link);
                                var element = driver.FindElement(By.XPath("//*[@id=\"collage-footer\"]/footer/div/div[2]/div/div/div[2]"));
                                Actions actions = new Actions(driver);
                                var fav = driver.FindElement(By.XPath("//*[@id=\"listing-right-column\"]/div/div[1]/div[1]/div/div/div[2]/div[1]/button"));
                                fav.Click();
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
    }
}
