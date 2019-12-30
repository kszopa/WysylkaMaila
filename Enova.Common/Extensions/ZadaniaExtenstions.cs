using Soneta.Core.Schedule;
using Soneta.Zadania;
using System;

namespace Enova.Common.Extensions
{
    public static class ZadaniaExtenstions
    {
        public static DateTime WyliczWywolanie(this Zadanie szablon)
        {
            var schedulerDefinition = (ScheduleDefinition)szablon.Features["PowiadomieniaDefinicjaHZ"];
            var definicjaCyklu = schedulerDefinition.CycleDefinition;

            return definicjaCyklu.Podglad.Data.GetValueOrDefault();
        }
    }
}
