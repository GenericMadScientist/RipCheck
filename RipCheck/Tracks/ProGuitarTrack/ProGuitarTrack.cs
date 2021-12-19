using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class ProGuitarTrack
    {
        private readonly Dictionary<Difficulty, IList<ProGuitarNote>> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;

        private Warnings trackWarnings = new Warnings();

        public ProGuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument)
        {
            name = instrument;
            tempoMap = _tempoMap;

            notes.Add(Difficulty.Easy, new List<ProGuitarNote>());
            notes.Add(Difficulty.Medium, new List<ProGuitarNote>());
            notes.Add(Difficulty.Hard, new List<ProGuitarNote>());
            notes.Add(Difficulty.Expert, new List<ProGuitarNote>());

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;
                byte velocity = note.Velocity;

                if (!Enum.IsDefined(typeof(ProGuitarTrackNote), key))
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

                if (key < 24 || key > 101 || (key % 12) > 5)
                {
                    continue;
                }

                if (velocity < 100 || velocity > 122)
                {
                    var position = note.Time;
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    trackWarnings.Add($"Invalid Pro Guitar fret number: note {key} with velocity {velocity} on {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                }

                Difficulty difficulty = (Difficulty)((key - 24) / 24);
                ProGuitarFretColour colour = (ProGuitarFretColour)(key % 24);
                ProGuitarFretNumber fretNumber = (ProGuitarFretNumber)velocity;
                notes[difficulty].Add(new ProGuitarNote(colour, fretNumber, note.Time, note.Length));
            }
        }

        public Warnings RunChecks()
        {
            trackWarnings.AddRange(CheckChordSnapping());
            trackWarnings.AddRange(CheckDisjointChords());
            return trackWarnings;
        }

        public Warnings CheckChordSnapping()
        {
            var warnings = new Warnings();

            foreach (KeyValuePair<Difficulty, IList<ProGuitarNote>> item in notes)
            {
                Difficulty difficulty = item.Key;
                long[] positions = item.Value.Select(n => n.Position).OrderBy(p => p).ToArray();
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
                        warnings.Add($"Chord snapping: {name} {difficulty} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                    }
                }
            }

            return warnings;
        }

        public Warnings CheckDisjointChords()
        {
            var warnings = new Warnings();

            foreach (KeyValuePair<Difficulty, IList<ProGuitarNote>> item in notes)
            {
                Difficulty difficulty = item.Key;
                (long, long)[] positionLengthPairs = item.Value.Select(n => (n.Position, n.Length)).OrderBy(p => p).ToArray();
                IEnumerable<((long, long), (long, long))> pairs = positionLengthPairs.Zip(positionLengthPairs.Skip(1));
                foreach (var ((earlyPos, earlyLength), (latePos, lateLength)) in pairs)
                {
                    if (earlyPos != latePos)
                    {
                        continue;
                    }
                    if (earlyLength == lateLength)
                    {
                        continue;
                    }
                    if (lateLength <= 160)
                    {
                        continue;
                    }
                    var lateTime = (MetricTimeSpan) TimeConverter.ConvertTo(latePos, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(latePos, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * lateTime.Hours + lateTime.Minutes;
                    int seconds = lateTime.Seconds;
                    int millisecs = lateTime.Milliseconds;
                    warnings.Add($"Disjoint chord: {name} {difficulty} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                }
            }

            return warnings;
        }
    }
}
