using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace RipCheck
{
    class ProGuitarTrack
    {
        private readonly Dictionary<Difficulty, IList<INote>> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        public ProGuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
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
                byte colour = (byte)(key % 24);
                ProGuitarFretNumber fretNumber = (ProGuitarFretNumber)velocity;
                notes[difficulty].Add(new ProGuitarNote(colour, fretNumber, note.Time, note.Length));
            }
        }

        public Warnings RunChecks(CheckOptions parameters)
        {
            foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
            {
                trackWarnings.AddRange(CommonChecks.CheckChordSnapping(difficulty.Key, difficulty.Value, name, tempoMap));
            }

            foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
            {
                trackWarnings.AddRange(CommonChecks.CheckOverlappingNotes(difficulty.Key, difficulty.Value, name, tempoMap));
            }

            if (parameters.Disjoints)
            {
                foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
                {
                    trackWarnings.AddRange(CommonChecks.CheckDisjointChords(difficulty.Key, difficulty.Value, name, tempoMap));
                }
            }

            return trackWarnings;
        }
    }
}
