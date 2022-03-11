using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class DrumsTrack
    {
        private readonly Dictionary<Difficulty, IList<INote>> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        public DrumsTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
        {
            name = instrument;
            tempoMap = _tempoMap;

            notes.Add(Difficulty.Easy, new List<INote>());
            notes.Add(Difficulty.Medium, new List<INote>());
            notes.Add(Difficulty.Hard, new List<INote>());
            notes.Add(Difficulty.Expert, new List<INote>());

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(DrumsTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", note.Time, tempoMap);
                        continue;
                    }
                }

                if (key < 60 || key > 100 || (key % 12) > 5)
                {
                    continue;
                }

                Difficulty difficulty = (Difficulty)((key - 60) / 12);
                byte colour = (byte)(key % 12);
                notes[difficulty].Add(new DrumsNote(colour, note.Time, note.Length));
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

            foreach (KeyValuePair<Difficulty, IList<INote>> item in notes)
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
    }
}
