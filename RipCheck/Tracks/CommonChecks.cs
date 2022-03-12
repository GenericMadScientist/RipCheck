using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    static class CommonChecks
    {
        public static Warnings CheckChordSnapping(Difficulty difficulty, IList<INote> notes, string name, TempoMap tempoMap)
        {
            var warnings = new Warnings();

            long[] positions = notes.Select(n => n.Position).OrderBy(p => p).ToArray();
            IEnumerable<(long, long)> gaps = positions.Zip(positions.Skip(1), (p, q) => (p, q - p));

            foreach (var (position, gap) in gaps)
            {
                if (gap > 0 && gap < 10)
                {
                    if (difficulty == Difficulty.None)
                    {
                        warnings.AddTimed($"Chord snapping: {name}", position, tempoMap);
                    }
                    else
                    {
                        warnings.AddTimed($"Chord snapping: {difficulty} on {name}", position, tempoMap);
                    }
                }
            }

            return warnings;
        }

        public static Warnings CheckDisjointChords(Difficulty difficulty, IList<INote> notes, string name, TempoMap tempoMap)
        {
            var warnings = new Warnings();

            IEnumerable<(INote, INote)> pairs = notes.Zip(notes.Skip(1));

            foreach (var (earlyNote, lateNote) in pairs)
            {
                if (earlyNote.Position != lateNote.Position)
                {
                    continue;
                }

                if (earlyNote.Length == lateNote.Length)
                {
                    continue;
                }

                if (lateNote.Length <= 160)
                {
                    continue;
                }

                if (difficulty == Difficulty.None)
                {
                    warnings.AddTimed($"Disjoint chord: {name}", lateNote.Position, tempoMap);
                }
                else
                {
                    warnings.AddTimed($"Disjoint chord: {difficulty} on {name}", lateNote.Position, tempoMap);
                }
            }

            return warnings;
        }

        public static Warnings CheckOverlappingNotes(Difficulty difficulty, IList<INote> notes, string name, TempoMap tempoMap)
        {
            var warnings = new Warnings();

            IEnumerable<(INote, INote)> pairs = notes.Zip(notes.Skip(1));

            foreach (var (earlyNote, lateNote) in pairs)
            {
                if (earlyNote.Note != lateNote.Note)
                {
                    continue;
                }

                if ((earlyNote.Position + earlyNote.Length) <= lateNote.Position)
                {
                    continue;
                }

                if (difficulty == Difficulty.None)
                {
                    warnings.AddTimed($"Overlapping note: {name}", lateNote.Position, tempoMap);
                }
                else
                {
                    warnings.AddTimed($"Overlapping note: {difficulty} on {name}", lateNote.Position, tempoMap);
                }
            }

            return warnings;
        }
    }
}
