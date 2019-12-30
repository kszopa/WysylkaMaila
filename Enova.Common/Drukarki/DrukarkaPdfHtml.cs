using Soneta.Business.UI;
using Soneta.Zadania;
using System.IO;

namespace Enova.Common.Drukarki
{
    public class DrukarkaPdfHtml
    {
        public FileStream Drukuj(Zadanie szablon, ReportResultFormat reportResultFormat)
        {
            var nazwaWzorca = szablon.Features.GetString("PowiadomieniaWydruk");
            var znajdowanieWzorca = new Szmaragd.Drukowanie.ZnajdowanieWzorcaASPX(szablon.Session);

            var sciezkaWzorca = znajdowanieWzorca.ZnajdzIZapiszWzorzecWydruku(nazwaWzorca);

            const string sciezkaZapisu = @"c:\wydruki\WysylkaMailiZWydrukiem\";
            Directory.CreateDirectory(sciezkaZapisu);

            var drukarka = new Szmaragd.Drukowanie.DrukarkaPDFHTML
            {
                SciezkaZWzorcem = sciezkaWzorca,
                SciezkaZapisuWydruku = sciezkaZapisu
            };

            var fileStream = drukarka.Drukuj(szablon, null, reportResultFormat);

            return fileStream;
        }
    }
}
