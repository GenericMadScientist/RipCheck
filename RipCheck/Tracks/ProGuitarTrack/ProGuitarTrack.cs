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
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        public ProGuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
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
                byte channel = note.Channel;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(ProGuitarTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", note.Time, tempoMap);
                        continue;
                    }
                }

                if (key < 24 || key > 101 || (key % 12) > 5)
                {
                    continue;
                }

                if (parameters.UnknownNotes)
                {
                    if (velocity < 100 || velocity > 122)
                    {
                        trackWarnings.AddTimed($"Invalid fret number: note {key} with velocity {velocity} on {name}", note.Time, tempoMap);
                        continue;
                    }

                    if (!Enum.IsDefined(typeof(ProGuitarTrackChannel), channel))
                    {
                        trackWarnings.AddTimed($"Unknown channel number: note {key} with channel {channel} on {name}", note.Time, tempoMap);
                    }
                }

                Difficulty difficulty = (Difficulty)((key - 24) / 24);
                ProGuitarFretColour colour = (ProGuitarFretColour)(key % 24);
                ProGuitarFretNumber fretNumber = (ProGuitarFretNumber)velocity;
                notes[difficulty].Add(new ProGuitarNote(colour, fretNumber, note.Time, note.Length));
            }
        }

        public Warnings RunChecks(CheckOptions parameters)
        {
            trackWarnings.AddRange(CheckChordSnapping());
            if (parameters.Disjoints)
            {
                trackWarnings.AddRange(CheckDisjointChords());
            }
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
                        trackWarnings.AddTimed($"Chord snapping: {difficulty} on {name}", position, tempoMap);
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
                    trackWarnings.AddTimed($"Disjoint chord: {difficulty} on {name}", latePos, tempoMap);
                }
            }

            return warnings;
        }
    }
}
