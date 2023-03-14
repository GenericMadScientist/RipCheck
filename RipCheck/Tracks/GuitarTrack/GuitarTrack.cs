using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace RipCheck
{
    class GuitarTrack
    {
        private readonly Dictionary<Difficulty, IList<INote>> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        private const int difficultiesStart = (int)GuitarTrackNote.EasyGreen;
        private const int difficultyRange = GuitarTrackNote.MediumGreen - GuitarTrackNote.EasyGreen;
        private const int difficultyMaxIndex = GuitarTrackNote.EasyOrange - GuitarTrackNote.EasyGreen;

        public GuitarTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
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
                var note = (GuitarTrackNote)key;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(GuitarTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }
                }

                if (note < GuitarTrackNote.EasyGreen || note > GuitarTrackNote.ExpertOrange ||
                    (key % difficultiesStart) > difficultyMaxIndex)
                {
                    continue;
                }

                Difficulty difficulty = Difficulty.Easy + ((key - difficultiesStart) / difficultyRange);
                byte colour = (byte)(key % difficultyRange);
                notes[difficulty].Add(new GuitarNote(colour, midiNote.Time, midiNote.Length));
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
                if (name != TrackNames.Keys)
                {
                    foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
                    {
                        trackWarnings.AddRange(CommonChecks.CheckDisjointChords(difficulty.Key, difficulty.Value, name, tempoMap));
                    }
                }
            }

            return trackWarnings;
        }
    }
}
