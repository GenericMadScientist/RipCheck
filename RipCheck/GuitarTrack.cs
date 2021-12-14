using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class GuitarTrack
    {
        private readonly Dictionary<Difficulty, IList<GuitarNote>> notes = new();
        private readonly string name;

        public GuitarTrack(TrackChunk track, string instrument)
        {
            name = instrument;

            notes.Add(Difficulty.Easy, new List<GuitarNote>());
            notes.Add(Difficulty.Medium, new List<GuitarNote>());
            notes.Add(Difficulty.Hard, new List<GuitarNote>());
            notes.Add(Difficulty.Expert, new List<GuitarNote>());

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;
                if (key < 60 || key > 100 || (key % 12) > 4)
                {
                    continue;
                }
                Difficulty difficulty = (Difficulty)((key - 60) / 12);
                GuitarFretColour colour = (GuitarFretColour)(key % 12);
                notes[difficulty].Add(new GuitarNote(colour, note.Time, note.Length));
            }
        }

        public Warnings CheckChordSnapping(TempoMap tempoMap)
        {
            var warnings = new Warnings();

            foreach (KeyValuePair<Difficulty, IList<GuitarNote>> item in notes)
            {
                Difficulty difficulty = item.Key;
                long[] positions = item.Value.Select(n => n.Position).OrderBy(p => p).ToArray();
                IEnumerable<(long, long)> gaps = positions.Zip(positions.Skip(1), (p, q) => (p, q - p));
                foreach (var (position, gap) in gaps)
                {
                    if (gap > 0 && gap < 10)
                    {
                        var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                        int minutes = 60 * time.Hours + time.Minutes;
                        int seconds = time.Seconds;
                        int millisecs = time.Milliseconds;
                        warnings.Add($"Chord snapping: {name} {difficulty} at {minutes}:{seconds:d2}.{millisecs:d3}");
                    }
                }
            }

            return warnings;
        }

        public Warnings CheckDisjointChords(TempoMap tempoMap)
        {
            var warnings = new Warnings();

            foreach (KeyValuePair<Difficulty, IList<GuitarNote>> item in notes)
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
                    int minutes = 60 * lateTime.Hours + lateTime.Minutes;
                    int seconds = lateTime.Seconds;
                    int millisecs = lateTime.Milliseconds;
                    warnings.Add($"Disjoint chord: {name} {difficulty} at {minutes}:{seconds:d2}.{millisecs:d3}");
                }
            }

            return warnings;
        }
    }
}
