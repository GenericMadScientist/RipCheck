using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace RipCheck
{
    class ProKeysTrack
    {
        private readonly List<INote> notes = new();
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }
        private readonly Difficulty difficulty;

        private readonly Warnings trackWarnings = new Warnings();

        public ProKeysTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
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

            foreach (Note midiNote in track.GetNotes())
            {
                byte key = midiNote.NoteNumber;
                var note = (ProKeysTrackNote)key;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(ProKeysTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }
                }

                if (note < ProKeysTrackNote.C1 || note > ProKeysTrackNote.C3)
                {
                    continue;
                }

                notes.Add(new ProKeysNote(key, midiNote.Time, midiNote.Length));
            }
        }

        public Warnings RunChecks()
        {
            trackWarnings.AddRange(CommonChecks.CheckChordSnapping(difficulty, notes, name, tempoMap));
            trackWarnings.AddRange(CommonChecks.CheckOverlappingNotes(difficulty, notes, name, tempoMap));
            return trackWarnings;
        }
    }
}
