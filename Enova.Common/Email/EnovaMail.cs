using System;
using Enova.Common.Drukarki;
using Enova.Common.Extensions;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Core.Schedule;
using Soneta.Zadania;

namespace Enova.Common.Email
{
   public class EnovaMail
    {
        public void Wyslij(Zadanie szablon)
        {
            var adres = szablon.Kontakt.EMAIL;
            var temat = szablon.Features.GetString("PowiadomieniaTemat");
            var body = szablon.Description;
            var drukarka = new DrukarkaPdfHtml();

            if (string.IsNullOrWhiteSpace(body))
            {
                var wydrukHtml = drukarka.Drukuj(szablon, ReportResultFormat.HTML);
                body = wydrukHtml.ToString();
            }

            var wydruk = drukarka.Drukuj(szablon, ReportResultFormat.PDF);

            var mail = new Soneta.Core.EnovaMail(szablon.Session);

            mail.AddTo(adres);
            mail.AddSubject(temat);
            mail.AddBody(body);
            if (wydruk != null)
                mail.AddAttachment("Raport", wydruk);

            var wywolanie = szablon.WyliczWywolanie();

            if (mail.SendMail())
            {
                using (ITransaction trans = szablon.Session.Logout(true))
                {
                    szablon.Features["PowiadomieniaOstatnieWywolanie"] = DateTime.Now.ToString();
                    szablon.Features["PowiadomieniaKolejneWywolanie"] = wywolanie.ToString();

                    trans.CommitUI();
                }
            }
        }
        
        private static DateTime WyliczWywolanie(Zadanie szablon)
        {
            var schedulerDefinition = (ScheduleDefinition)szablon.Features["PowiadomieniaDefinicjaHZ"];
            var definicjaCyklu = schedulerDefinition.CycleDefinition;

            return definicjaCyklu.Podglad.Data.GetValueOrDefault();
        }
    }
}
