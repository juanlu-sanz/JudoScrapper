using System;
using System.Collections.Generic;

namespace JudoScrapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Scrapper scrapper = new Scrapper("https://www.fmjudo.es/index.php/component/content/article/18-clubes/3-clubes?zona=1");
            IEnumerable<string> urls = scrapper.RetrieveGymUrls();
            IEnumerable<Gym> gyms = scrapper.RetrieveGyms(urls);
            scrapper.SaveToCsv(gyms);
            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}