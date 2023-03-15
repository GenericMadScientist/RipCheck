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
        private bool checkExpertPlus = false;

        private readonly Warnings trackWarnings = new Warnings();

        private const int difficultiesStart = (int)DrumsTrackNote.EasyKick - 1; // -1 to account for 2x Kick
        private const int difficultyRange = DrumsTrackNote.MediumKick - DrumsTrackNote.EasyKick;
        private const int difficultyMaxIndex = DrumsTrackNote.Easy5LaneGreen - DrumsTrackNote.EasyKick;

        public DrumsTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
        {
            name = instrument;
            tempoMap = _tempoMap;
            checkExpertPlus = parameters.GHExpertPlus;

            notes.Add(Difficulty.Easy, new List<INote>());
            notes.Add(Difficulty.Medium, new List<INote>());
            notes.Add(Difficulty.Hard, new List<INote>());
            notes.Add(Difficulty.Expert, new List<INote>());

            foreach (Note midiNote in track.GetNotes())
            {
                byte key = midiNote.NoteNumber;
                var note = (DrumsTrackNote)key;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(DrumsTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }
                }

                if (note < DrumsTrackNote.EasyKick || note > DrumsTrackNote.Expert5LaneGreen)
                {
                    continue;
                }

                if (note != DrumsTrackNote.ExpertPlusKick && (key % difficultyRange) > difficultyMaxIndex)
                {
                    continue;
                }

                Difficulty difficulty = Difficulty.Easy + ((key - difficultiesStart) / difficultyRange);
                byte colour = (byte)(key % difficultyRange);
                notes[difficulty].Add(new DrumsNote(colour, midiNote.Time, midiNote.Length));
            }
        }

        public Warnings CheckExpertPlus()
        {
            var warnings = new Warnings();

            var diff = notes[Difficulty.Expert];
            var kicks = diff.Where((note) => note.Note == (byte)DrumsFretColour.Kick);
            var doubleKickPositions = diff.Where((note) => note.Note == (byte)DrumsFretColour.DoubleKick)
                .Select((note) => note.Position).ToHashSet();

            foreach (var kick in kicks)
            {
                if (!doubleKickPositions.Contains(kick.Position))
                {
                    warnings.AddTimed($"Found Expert kick with no corresponding Expert+ kick on {name}", kick.Position, tempoMap);
                }
            }

            return warnings;
        }

        public Warnings RunChecks()
        {
            foreach (KeyValuePair<Difficulty, IList<INote>> difficulty in notes)
            {
                trackWarnings.AddRange(CommonChecks.CheckChordSnapping(difficulty.Key, difficulty.Value, name, tempoMap));
                trackWarnings.AddRange(CommonChecks.CheckOverlappingNotes(difficulty.Key, difficulty.Value, name, tempoMap));
            }

            if (checkExpertPlus)
            {
                trackWarnings.AddRange(CheckExpertPlus());
            }

            return trackWarnings;
        }
    }
}
