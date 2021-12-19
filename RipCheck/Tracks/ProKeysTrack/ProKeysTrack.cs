using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class ProKeysTrack
    {
        private readonly List<ProKeysNote> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        private readonly Difficulty difficulty;

        private Warnings trackWarnings = new Warnings();

        public ProKeysTrack(TrackChunk track, TempoMap _tempoMap, string instrument)
        {
            name = instrument;
            tempoMap = _tempoMap;
            switch (name[name.Length - 1])
            {
                case 'E':
                    difficulty = Difficulty.Easy;
                    break;
                case 'M':
                    difficulty = Difficulty.Medium;
                    break;
                case 'H':
                    difficulty = Difficulty.Hard;
                    break;
                case 'X':
                    difficulty = Difficulty.Expert;
                    break;
            }

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;

                if (!Enum.IsDefined(typeof(ProKeysTrackNotes), key))
                {
                    var position = note.Time;
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    trackWarnings.Add($"Unknown note: {key} on {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                    continue;
                }

                if (key < 48 || key > 72)
                {
                    continue;
                }

                notes.Add(new ProKeysNote(key, note.Time, note.Length));
            }
        }

        public Warnings RunChecks()
        {
            trackWarnings.AddRange(CheckChordSnapping());
            return trackWarnings;
        }

        public Warnings CheckChordSnapping()
        {
            var warnings = new Warnings();

            long[] positions = notes.Select(n => n.Position).OrderBy(p => p).ToArray();
            IEnumerable<(long, long)> gaps = positions.Zip(positions.Skip(1), (p, q) => (p, q - p));
            foreach (var (position, gap) in gaps)
            {
                if (gap > 0 && gap < 10)
                {
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    warnings.Add($"Chord snapping: {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                }
            }

            return warnings;
        }
    }
}
