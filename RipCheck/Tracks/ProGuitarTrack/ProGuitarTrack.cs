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

        private const int difficultiesStart = (int)ProGuitarTrackNote.EasyRed;
        private const int difficultyRange = ProGuitarTrackNote.MediumRed - ProGuitarTrackNote.EasyRed;
        private const int difficultyMaxIndex = ProGuitarTrackNote.EasyPurple - ProGuitarTrackNote.EasyRed;

        public ProGuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
        {
            name = instrument;
            tempoMap = _tempoMap;

            notes.Add(Difficulty.Easy, new List<INote>());
            notes.Add(Difficulty.Medium, new List<INote>());
            notes.Add(Difficulty.Hard, new List<INote>());
            notes.Add(Difficulty.Expert, new List<INote>());

            foreach (Note midiNote in track.GetNotes())
            {
                byte key = midiNote.NoteNumber;
                byte velocity = midiNote.Velocity;
                byte channel = midiNote.Channel;
                var note = (ProGuitarTrackNote)key;
                var fret = (ProGuitarFretNumber)velocity;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(ProGuitarTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }
                }

                if (note < ProGuitarTrackNote.EasyRed || note > ProGuitarTrackNote.ExpertPurple ||
                    (key % difficultyRange) > difficultyMaxIndex)
                {
                    continue;
                }

                if (parameters.UnknownNotes)
                {
                    if (fret < ProGuitarFretNumber.Fret0 || fret > ProGuitarFretNumber.Fret22)
                    {
                        trackWarnings.AddTimed($"Invalid fret number: note {key} with velocity {velocity} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }

                    if (!Enum.IsDefined(typeof(ProGuitarTrackChannel), channel))
                    {
                        trackWarnings.AddTimed($"Unknown channel number: note {key} with channel {channel} on {name}", midiNote.Time, tempoMap);
                    }
                }

                Difficulty difficulty = Difficulty.Easy + ((key - difficultiesStart) / difficultyRange);
                byte colour = (byte)(key % difficultyRange);
                notes[difficulty].Add(new ProGuitarNote(colour, fret, midiNote.Time, midiNote.Length));
            }
        }

        public Warnings RunChecks(CheckOptions parameters)
        {
            foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
            {
                trackWarnings.AddRange(CommonChecks.CheckChordSnapping(difficulty.Key, difficulty.Value, name, tempoMap));
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
