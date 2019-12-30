using Soneta.Business;
using Soneta.Business.Db;
using Soneta.CRM;
using Soneta.CRM.Config;
using Soneta.Zadania;
using System;
using System.IO;
using System.Linq;

namespace Enova.Common.Email
{
    public class PocztaEnova
    {
        private readonly Row row;
        private readonly KontoPocztowe konto;

        public PocztaEnova(Row row)
        {
            RowCondition rc = new FieldCondition.Like("Nazwa", "automat%");
            this.konto = CRMModule.GetInstance(row).KontaPocztowe.WgNazwa[rc].Cast<KontoPocztowe>().FirstOrDefault();
            if (konto == null)
            {
                Console.WriteLine("Nie znalazłem");
                konto = ZadaniaModule.GetInstance(row.Session).Config.Operatorzy.Aktualny.DomyslneKontoPocztowe;
            }
            Console.WriteLine(konto.Nazwa);
            this.row = row;
        }

        public WiadomoscRobocza UtworzWiadomosc(string emailTo, string tresc, string temat, FileStream stream)
        {
            WiadomoscRobocza wiadomoscRobocza = null;

            
            using (ITransaction transaction = row.Session.Logout(true))
            {
                wiadomoscRobocza = new WiadomoscRobocza();
                wiadomoscRobocza.KontoPocztowe = konto;
                row.Session.GetCRM().WiadomosciEmail.AddRow((Row)wiadomoscRobocza);

        
                IEmailElement iElement = this.Element();
                if (iElement != null)
                {
                    Console.WriteLine("Dodaje element");
                    wiadomoscRobocza.Session.GetCRM().ElementyEmail.AddRow(new ElementEmail
                    {
                        WiadomoscEmail = wiadomoscRobocza,
                        Element = this.Element()
                    });
                }

                //this.KopiujZalacznikiDoWysylki(wiadomoscRobocza);
                KopiujZalaczniki(wiadomoscRobocza, stream);

                wiadomoscRobocza.Do = emailTo;
                wiadomoscRobocza.KontoPocztowe = konto;
                wiadomoscRobocza.Temat = temat;
                wiadomoscRobocza.Tresc = tresc;
                wiadomoscRobocza.DostepnyHtml = true;
                wiadomoscRobocza.TypWiadomosci = TypWiadomości.Robocza;

                transaction.Commit();
            }
            return wiadomoscRobocza;
        }

        public WiadomoscRobocza WyslijWiadomosc(string emailTo, string tresc, string temat, bool wyslac, FileStream stream)
        {
            WiadomoscRobocza wiadomoscRobocza;

            using (ITransaction transaction = row.Session.Logout(true))
            {
                wiadomoscRobocza = new WiadomoscRobocza();
                CRMModule.GetInstance(row).WiadomosciEmail.AddRow(wiadomoscRobocza);
                IEmailElement iElement = this.Element();
                if (iElement != null)
                {
                    wiadomoscRobocza.Session.GetCRM().ElementyEmail.AddRow(new ElementEmail
                    {
                        WiadomoscEmail = wiadomoscRobocza,
                        Element = this.Element()
                    });
                }

                KopiujZalaczniki(wiadomoscRobocza, stream);

                wiadomoscRobocza.Do = emailTo;
                wiadomoscRobocza.KontoPocztowe = konto;
                wiadomoscRobocza.Temat = temat;
                wiadomoscRobocza.Tresc = tresc;
                wiadomoscRobocza.DostepnyHtml = true;
                wiadomoscRobocza.TypWiadomosci = TypWiadomości.Robocza;

                transaction.Commit();
            }
            if (wyslac)
                this.WyslijWiadomosc(wiadomoscRobocza);

            return wiadomoscRobocza;
        }

        private static void KopiujZalaczniki(WiadomoscRobocza wiadomosc, FileStream stream)
        {
            var businessModule = BusinessModule.GetInstance(wiadomosc.Session);
            var zalacznik = new Attachment(wiadomosc, AttachmentType.Attachments)
            {
                Name = "Raport.pdf"
            };

            businessModule.Attachments.AddRow(zalacznik);

            zalacznik.LoadFromStream(stream);
            zalacznik.LoadIconFromFile(stream.Name);
        }

        public void WyslijWiadomosc(WiadomoscEmail wiadomosc)
        {
            //if (!TrybTestowy.CzyKomputerTestowy())
            //{
            MailHelper.SendMessage(wiadomosc);
            //}

        }

        private IEmailElement Element()
        {
            IEmailElement el = row as IEmailElement;
            if (el != null)
            {
                return el;
            }

            return null;
        }
    }
}
