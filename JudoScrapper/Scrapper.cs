using CsvHelper;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JudoScrapper
{
    internal class Scrapper
    {
        private string titleClass = ".contentheading";
        private string contentClass = ".jsn-article-content";

        private ScrapingBrowser browser;
        private Uri mainuri = null;
        private WebPage listPage = null;

        public Scrapper(string url)
        {
            browser = new ScrapingBrowser();
            browser.AllowAutoRedirect = true; // Browser has settings you can access in setup
            browser.AllowMetaRedirect = true;
            browser.Encoding = new System.Text.UTF8Encoding();

            mainuri = new Uri(url);
            listPage = browser.NavigateToPage(mainuri);
            HtmlNode titleNode = listPage.Html.CssSelect(titleClass).First();
            string pageTitle = titleNode.InnerText;
            Console.WriteLine(String.Format("Initial page load complete! {0} loaded", pageTitle.Trim()));
        }

        public IEnumerable<string> RetrieveGymUrls()
        {
            if (listPage == null || mainuri == null)
            {
                throw new ArgumentException("Cannot retrieve anything from a null list!");
            }
            HtmlNode listNode = listPage.Html.CssSelect(contentClass).First();
            List<string> urls = listNode.ChildNodes.Where(
                n => n.Name
                .Equals("a", StringComparison.InvariantCultureIgnoreCase))
                .Select(n => mainuri.Scheme + "://" + mainuri.Host + "/" + n.Attributes["href"].Value)
                .ToList();

            return urls;
        }

        public IEnumerable<Gym> RetrieveGyms(IEnumerable<string> urls)
        {
            List<Gym> results = new List<Gym>();
            for (int i = 0; i < urls.Count(); i++)
            {
                string url = urls.ElementAt(i);

                WebPage page = browser.NavigateToPage(new Uri(url));
                HtmlNode content = page.Html.CssSelect(contentClass).First();
                List<HtmlNode> properties = content.ChildNodes.ToList();

                int index = 0;
                Gym gym = new Gym() { Url = url };
                //Manually fetch properties
                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Nombre:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.Name = FirstLetterToUpper(properties.ElementAt(index + 1).InnerText.Trim().ToLower());

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Domicilio:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.Address = properties.ElementAt(index + 1).InnerText.Replace("nº", "").Replace("º", "").Trim() + ", Madrid, Spain";

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Codigo postal:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.ZipCode = properties.ElementAt(index + 1).InnerText.Trim();

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Poblacion:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.City = properties.ElementAt(index + 1).InnerText.Trim();

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Horario:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.Hours = properties.ElementAt(index + 1).InnerText.Trim();

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Telefono:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.Phones = properties.ElementAt(index + 1).InnerText.Trim();

                index = properties.FindIndex(n =>
                    n.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase) &&
                    n.InnerText.Equals("Web:", StringComparison.InvariantCultureIgnoreCase)
                );
                gym.GymWeb = properties.ElementAt(index + 1).InnerText.Trim();
                results.Add(gym);
                Console.WriteLine("Processed Gym " + gym.Name + " (" + (i + 1) + " of " + urls.Count() + ")");
            }

            return results;
        }

        public void SaveToCsv(IEnumerable<Gym> gyms)
        {
            using (var csv = new CsvWriter(new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/gyms.csv")))
            {
                csv.Configuration.Encoding = new System.Text.UTF8Encoding();
                csv.WriteRecords(gyms);
            }
        }

        private string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}