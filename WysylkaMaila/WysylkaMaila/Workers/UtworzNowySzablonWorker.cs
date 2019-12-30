using Soneta.Business;
using Soneta.Business.Db;
using Soneta.Core;
using Soneta.Core.Schedule;
using Soneta.Types;
using Soneta.Zadania;
using System;
using WysylkaMaila.Workers;

[assembly: Worker(typeof(UtworzNowySzablonWorker), typeof(Zadania))]
namespace WysylkaMaila.Workers
{
    public class UtworzNowySzablonWorker
    {
        [Context]
        public Context Context { get; set; }

        [Action("Utwórz szablon",
            Priority = 1,
            Icon = ActionIcon.Wizard,
            Target = ActionTarget.ToolbarWithText | ActionTarget.Menu,
            Mode = ActionMode.SingleSession)]
        public Zadanie UtworzNowySzablon()
        {
            var szablon = UtworzDokument(Context);
            var scheduleDefinition = UtworzScheduleDefinition(Context, szablon);
            
            using (var session = Context.Session.Login.CreateSession(false, true, "UstawCechySzablonu"))
            {
                var szablonSession = (Zadanie)session[szablon];

                using (ITransaction transaction = session.Logout(true))
                {
                    szablonSession.Features["PowiadomieniaDefinicjaHZ"] = scheduleDefinition;
                    szablonSession.Features["PowiadomieniaOstatnieWywolanie"] = DateTime.MinValue.ToString();

                    transaction.CommitUI();
                }

                session.Save();

                return szablonSession;
            }
        }

        private static ScheduleDefinition UtworzScheduleDefinition(Context context, Zadanie szablon)
        {
            var scheduleDefinition = new ScheduleDefinition();

            using (var session = context.Session.Login.CreateSession(false, true, "ScheduleDefinition"))
            {
                var coreModule = CoreModule.GetInstance(session);
                var businessModule = BusinessModule.GetInstance(session);

                var taskDefinition = businessModule.TaskDefs.ByName["CfgNodes", "WysylkaMailiZWydrukiem"];

                using (var transaction = session.Logout(true))
                {
                    scheduleDefinition.Name = $"Scheduler_{szablon.NumerPelny}";
                    scheduleDefinition.TaskDefinition = taskDefinition;

                    coreModule.ScheduleDefs.AddRow(scheduleDefinition);

                    transaction.CommitUI();
                }

                session.Save();
            }

            return scheduleDefinition;
        }

        private static Zadanie UtworzDokument(Context context)
        {
            var szablon = new Zadanie();

            using (var session = context.Session.Login.CreateSession(false, true, "UtworzDokument"))
            {
                var module = ZadaniaModule.GetInstance(session);
                var definicja = module.DefZadan.WgSymbolu["SM"];

                using (var transaction = session.Logout(true))
                {
                    szablon.Definicja = definicja;
                    szablon.Nazwa = "Szablon maila";

                    module.Zadania.AddRow(szablon);

                    transaction.CommitUI();
                }

                session.Save();
            }

            return szablon;
        }

        public static bool IsVisibleUtworzNowySzablon(Context context)
        {
            var uiLocation = (UILocation)context[typeof(UILocation)];

            return uiLocation != null && uiLocation.FolderPath.Contains("CRM/Zdarzenia");
        }
    }
}
