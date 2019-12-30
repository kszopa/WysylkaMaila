using Soneta.Business;
using Soneta.Core.Schedule;
using Soneta.Core.UI;
using Soneta.Zadania;
using System.Collections.Generic;
using Soneta.Core;
using WysylkaMaila.UI.UI;

[assembly: Worker(typeof(SzablonMailaExtender))]
namespace WysylkaMaila.UI.UI
{
    public class SzablonMailaExtender
    {
        [Context]
        public Zadanie Zadanie { get; set; }

        public ScheduleDefinition ScheduleDefinition => (ScheduleDefinition)this.Zadanie.Features["PowiadomieniaDefinicjaHZ"];

        public DefinicjaCyklu Edit()
        {
            return this.ScheduleDefinition?.CycleDefinition;
        }

        public string CycleDefinition()
        {
            return this.ScheduleDefinition == null ? string.Empty : this.ScheduleDefinition.CycleDefinition.ToString();
        }

        public bool IsReadOnly()
        {
            return this.ScheduleDefinition == null || this.ScheduleDefinition.IsFileWatcher();
        }
        
        public bool IsVisible() => this.Zadanie.Definicja.Symbol == "SM";
    }
}
