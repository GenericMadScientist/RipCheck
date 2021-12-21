using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

namespace RipCheck
{
    public class Warnings
    {
        private readonly List<string> warnings;

        public Warnings()
        {
            warnings = new List<string>();
        }

        public void Add(string warning)
        {
            warnings.Add(warning);
        }

        public void AddTimed(string message, long position, TempoMap tempoMap)
        {
            var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
            var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
            int minutes = 60 * time.Hours + time.Minutes;
            int seconds = time.Seconds;
            int millisecs = time.Milliseconds;
            warnings.Add($"{message} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars + 1}.{ticks.Beats + 1}.{ticks.Ticks})");
                                                                                    // Add one to bars and beats because they're reported 0-indexed while REAPER shows them 1-indexed
        }

        public void AddRange(Warnings newWarningSet)
        {
            if (newWarningSet is not null)
            {
                warnings.AddRange(newWarningSet.warnings);
            }
        }

        public void PrintWarnings()
        {
            foreach (string warning in warnings)
            {
                Console.WriteLine(warning);
            }
        }

        public bool IsEmpty()
        {
            return warnings.Count == 0;
        }
    }
}
