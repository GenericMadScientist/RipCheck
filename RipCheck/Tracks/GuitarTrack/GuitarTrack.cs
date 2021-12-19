using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class GuitarTrack
    {
        private readonly Dictionary<Difficulty, IList<GuitarNote>> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        public GuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument, Options parameters)
        {
            name = instrument;
            tempoMap = _tempoMap;

            notes.Add(Difficulty.Easy, new List<GuitarNote>());
            notes.Add(Difficulty.Medium, new List<GuitarNote>());
            notes.Add(Difficulty.Hard, new List<GuitarNote>());
            notes.Add(Difficulty.Expert, new List<GuitarNote>());

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(GuitarTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", note.Time, tempoMap);
                        continue;
                    }
                }

                if (key < 60 || key > 100 || (key % 12) > 4)
                {
                    continue;
                }

                Difficulty difficulty = (Difficulty)((key - 60) / 12);
                GuitarFretColour colour = (GuitarFretColour)(key % 12);
                notes[difficulty].Add(new GuitarNote(colour, note.Time, note.Length));
            }
        }

        public Warnings RunChecks(Options parameters)
        {
            if (!parameters.NoChordSnapping)
            {
                trackWarnings.AddRange(CheckChordSnapping());
            }
            if (!parameters.NoDisjoints)
            {
                if (name != "PART KEYS")
                {
                    trackWarnings.AddRange(CheckDisjointChords());
                }
            }

            return trackWarnings;
        }

        public Warnings CheckChordSnapping()
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
                        trackWarnings.AddTimed($"Chord snapping: {difficulty} on {name}", position, tempoMap);
                    }
                }
            }

            return warnings;
        }

        public Warnings CheckDisjointChords()
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
                    trackWarnings.AddTimed($"Disjoint chord: {difficulty} on {name}", latePos, tempoMap);
                }
            }

            return warnings;
        }
    }
}
