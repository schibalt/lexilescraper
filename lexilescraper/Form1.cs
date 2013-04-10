using System.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using HtmlAgilityPack;
using System.Net;

namespace lexilescraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// this function is called when the select file button is pushed.  makes a dialog for picking the 
        /// html file and then writes the path to it in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog1.Filter = "HTML files|*.htm;*.html";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
        }

        /// <summary>
        /// this is called when the scrape button is pushed.  it parses the file into a collection of htmlnodes.
        /// the htmlnode class is part of the html agility pack which exists to facilitate html parsing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Stream myStream = null;

            // convert string to stream
            byte[] filenameByteArray = Encoding.UTF8.GetBytes(textBox1.Text);
            MemoryStream filename = new MemoryStream(filenameByteArray);
            myStream = filename;

            if (myStream != null)
                using (myStream)
                {
                    List<string> hrefTags = new List<string>();

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.Load(textBox1.Text);

                    HtmlNodeCollection bookInfoNodes = doc.DocumentNode.SelectNodes("//div[@class='booksInfo']");
                    int bookCounter = 2;

                    foreach (HtmlNode book in bookInfoNodes)
                    {
                        //string score = book.SelectSingleNode("//div[@class='booksLft fltlft']/ul/li").InnerText.Trim();
                        string detailPageUrl = book.SelectSingleNode("//*[@id=\"book_results\"]/div[" + bookCounter + "]/div[2]/div/h1/a").Attributes["href"].Value;
                        //string title = book.SelectSingleNode("//div[@class='booksMid fltlft']/div[@class='midInfo']/h1/a").InnerText.Replace("\r\n", "");
                        //string author = book.SelectSingleNode("//div[@class='booksMid fltlft']/div[@class='midInfo']/span").InnerText.Replace("by:", "").Trim() ;
                        //string description = book.SelectSingleNode("//div[@class='booksMid fltlft']/div[@class='midInfo']/p").InnerHtml.Replace("\r\n", "").Trim();
                        //string pages = book.SelectSingleNode("//div[@class='booksMid fltlft']/div[@class='midInfo']/p[@class='booksPages']").InnerHtml.Replace(@"<span>Pages:</span>", "").Trim();
                        //string isbn13 = book.SelectSingleNode("//div[@class='booksRgt fltrgt']/div[@class='mt10']/p").InnerHtml.Replace(@"<strong>ISBN13:</strong>", "").Trim();

                        HtmlWeb web = new HtmlWeb();
                        HtmlAgilityPack.HtmlDocument document;

                        //this is for use behind a proxy
                        if (noProxButton.Checked) document = web.Load(detailPageUrl);
                        else document = web.Load(detailPageUrl, "proxy url", 8080, "username", GetProxyPassword());

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(detailPageUrl);

                        ScrapeBookPage(document);
                        bookCounter++;
                    }
                }
        }

        #region appcfgencryption
        private string GetProxyPassword()
        {
            // Get the application configuration file.
            System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            // Get the section to unprotect.
            ConfigurationSection connStrings =
                config.AppSettings;

            if (connStrings != null)
            {
                if (connStrings.SectionInformation.IsProtected)
                {
                    if (!connStrings.ElementInformation.IsLocked)
                    {
                        // Unprotect the section.
                        connStrings.SectionInformation.UnprotectSection();

                        connStrings.SectionInformation.ForceSave = true;
                        config.Save(ConfigurationSaveMode.Full);

                        Console.WriteLine("Section {0} is now unprotected.",
                            connStrings.SectionInformation.Name);

                        Console.WriteLine("appsettings:\n" + connStrings.SectionInformation.GetRawXml()); 
                    }
                    else
                        Console.WriteLine(
                             "Can't unprotect, section {0} is locked",
                             connStrings.SectionInformation.Name);
                }
                else
                    Console.WriteLine(
                        "Section {0} is already unprotected.",
                        connStrings.SectionInformation.Name);

            }
            else
                Console.WriteLine("Can't get the section {0}",
                    connStrings.SectionInformation.Name);

            return "";
        }
        #endregion

        /// <summary>
        /// this scrapes each book metadata field into a local variable
        /// </summary>
        /// <param name="detailPage"></param>
        private void ScrapeBookPage(HtmlAgilityPack.HtmlDocument detailPage)
        {
            HtmlNode titleNode = detailPage.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div/div[2]/h1");
            HtmlNode author = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/ul/li/a");
            HtmlNode lexileMeasure = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[1]/div[1]/h2");
            HtmlNode pages = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[1]/div[2]/h2");
            HtmlNode summary = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[2]/p");
            HtmlNode language = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[3]/div[2]/p");
            //HtmlNode bookType = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[3]/div[4]");
            HtmlNode publisher = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[4]/div/div[1]/span[2]");
            HtmlNode copyright = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[4]/div/div[2]/span[2]");
            HtmlNode isbn = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[4]/div/div[3]/span[2]");
            HtmlNode isbn13 = detailPage.DocumentNode.SelectSingleNode("//*[@id=\"book-wrapper\"]/div[2]/div[4]/div/div[4]/span[2]");

            string svTitle;
            string svAuthor;
            string svLexileMeasure;
            string svPages;
            string svSummary;
            string svLanguage;
            // string svBookType ;
            string svPublisher;
            string svCopyright;
            string svIsbn;
            string svIsbn13;

            if (null != titleNode.InnerHtml) svTitle = titleNode.InnerHtml;
            if (null != author) if (null != author.InnerHtml) svAuthor = author.InnerHtml;
            if (null != lexileMeasure.InnerHtml) svLexileMeasure = lexileMeasure.InnerHtml;
            if (null != pages) if (null != pages.InnerHtml) svPages = pages.InnerHtml;
            if (null != summary.InnerHtml) svSummary = summary.InnerHtml;
            if (null != language.InnerHtml) svLanguage = language.InnerHtml;
            // if(null !=bookType.InnerHtml) svBookType=bookType.InnerHtml;
            if (null != publisher.InnerHtml) svPublisher = publisher.InnerHtml;
            if (null != copyright.InnerHtml) svCopyright = copyright.InnerHtml;
            if (null != isbn.InnerHtml) svIsbn = isbn.InnerHtml;
            if (null != isbn13.InnerHtml) svIsbn13 = isbn13.InnerHtml;
        }

        /// <summary>
        /// this is supposed to encrypt the app.cponfig because it had my password in it 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            // Get the application configuration file.
            System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            // Define the Rsa provider name. 
            string provider =
                "RsaProtectedConfigurationProvider";

            // Get the section to protect.
            ConfigurationSection connStrings =
                config.AppSettings;

            if (connStrings != null)
            {
                if (!connStrings.SectionInformation.IsProtected)
                {
                    if (!connStrings.ElementInformation.IsLocked)
                    {
                        // Protect the section.
                        connStrings.SectionInformation.ProtectSection(provider);

                        connStrings.SectionInformation.ForceSave = true;
                        config.Save(ConfigurationSaveMode.Full);

                        Console.WriteLine("Section {0} is now protected by {1}",
                            connStrings.SectionInformation.Name,
                            connStrings.SectionInformation.ProtectionProvider.Name);

                    }
                    else
                        Console.WriteLine(
                             "Can't protect, section {0} is locked",
                             connStrings.SectionInformation.Name);
                }
                else
                    Console.WriteLine(
                        "Section {0} is already protected by {1}",
                        connStrings.SectionInformation.Name,
                        connStrings.SectionInformation.ProtectionProvider.Name);

            }
            else
                Console.WriteLine("Can't get the section {0}",
                    connStrings.SectionInformation.Name);

            config.Save(ConfigurationSaveMode.Minimal,true);
        }
    }
}
