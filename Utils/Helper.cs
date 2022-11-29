using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETSYBUYER.Utils
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class SearchPair
    {
        public string Id { get; set; }
        public string SearchKey { get; set; }
    }
    public class Utils
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
                email.SendKeys("huynhnx93@gmail.com");
                Thread.Sleep(1000);
                var password = driver.FindElement(By.XPath("//*[@id=\"join_neu_password_field\"]"));
                password.SendKeys("Nguyenhuynh1993");
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
        

    }
}
