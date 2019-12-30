using Enova.Common.Drukarki;
using Enova.Common.Email;
using Enova.Common.Extensions;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Zadania;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WysylkaMaila.Workers;

[assembly: Worker(typeof(WyslijEmailZSzablonuWorker))]
namespace WysylkaMaila.Workers
{
    public class WyslijEmailZSzablonuWorker
    {
        public void WyslijEmail(Session session)
        {
            var szablony = PobierzAktywneSzablony(session);

            if (szablony.Count == 0)
                return;

            foreach (Zadanie sz in szablony)
            {
                var szablon = sz;

                var ostatnieWywolanie = DateTime.Parse(szablon.Features.GetString("PowiadomieniaOstatnieWywolanie"));
                var kolejneWywolanie = DateTime.Parse(szablon.Features.GetString("PowiadomieniaKolejneWywolanie"));

                var czyWysylac = CzyWysylac(ostatnieWywolanie, kolejneWywolanie);
                
                if (czyWysylac)
                {
                   
                    using (var nowaSesja = szablon.Session.Login.CreateSession(false, true, "WysylkaEMAIL"))
                    {
                        szablon = nowaSesja[szablon] as Zadanie;
                        var mail = PrzygotujMaila(szablon);
                        var pocztaEnova = new PocztaEnova(szablon);
                        var wiadomosc = pocztaEnova.UtworzWiadomosc(mail.EmailTo, mail.Tresc, mail.Temat, mail.Stream);
                        Console.WriteLine(wiadomosc.ID + "" + wiadomosc.State);
                        pocztaEnova.WyslijWiadomosc(wiadomosc);

                        nowaSesja.Save();
                        Console.WriteLine(wiadomosc.ID+""+wiadomosc.State);
                    }


                    UstawCechy(szablon);
                }
            }
        }

        private static void UstawCechy(Zadanie szablon)
        {
            var wywolanie = szablon.WyliczWywolanie();

            using (var session = szablon.Session.Login.CreateSession(false, true, "AktualizujCechy"))
            {
                using (ITransaction trans = session.Logout(true))
                {
                    var szablonSession = (Zadanie)session[szablon];

                    szablonSession.Features["PowiadomieniaOstatnieWywolanie"] = DateTime.Now.ToString();
                    szablonSession.Features["PowiadomieniaKolejneWywolanie"] = wywolanie.ToString();

                    trans.CommitUI();
                }

                session.Save();
            }
        }

        private static bool CzyWysylac(DateTime ostatnieWywolanie, DateTime kolejneWywolanie)
        {
            return ostatnieWywolanie < kolejneWywolanie && kolejneWywolanie <= DateTime.Now;
        }

        private static List<Zadanie> PobierzAktywneSzablony(Session session)
        {
            var zadaniaModule = ZadaniaModule.GetInstance(session);

            var defSzablonu = zadaniaModule.DefZadan.WgSymbolu["SM"];
            var zadania = zadaniaModule.Zadania.WgDefinicja;

            var rowCondition = RowCondition.Empty;

            rowCondition |= new FieldCondition.Equal("Definicja", defSzablonu);
            rowCondition |= new FieldCondition.Equal("Features.PowiadomieniaZablokowany", true);

            return zadania[rowCondition].ToList();
        }

        private static Mail PrzygotujMaila(Zadanie szablon)
        {
            var wydrukWorker = new DrukarkaPdfHtml();
            var tresc = szablon.Description;

            if (string.IsNullOrWhiteSpace(tresc))
            {
                var streamReader = new StreamReader(wydrukWorker.Drukuj(szablon, ReportResultFormat.HTML));
                tresc = streamReader.ReadToEnd();
            }

            return new Mail
            {
                EmailTo = szablon.Kontakt.EMAIL,
                Temat = szablon.Features.GetString("PowiadomieniaTemat"),
                Tresc = tresc,
                Stream = wydrukWorker.Drukuj(szablon, ReportResultFormat.PDF)
            };
        }
    }

    public struct Mail
    {
        public string EmailTo { get; set; }
        public string Tresc { get; set; }
        public string Temat { get; set; }
        public FileStream Stream { get; set; }
    }
}
