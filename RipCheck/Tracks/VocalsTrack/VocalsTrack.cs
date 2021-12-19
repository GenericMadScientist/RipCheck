using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RipCheck
{
    class VocalsTrack
    {
        private readonly List<VocalsNote> notes = new();
        private readonly Dictionary<long, string> lyrics = new();
        public readonly List<(long, long)> phrases = new();
        private readonly TempoMap tempoMap;
        private readonly string name;

        private Warnings trackWarnings = new Warnings();

        public VocalsTrack(TrackChunk track, TempoMap _tempoMap, string instrument, Options parameters)
        {
            name = instrument;
            tempoMap = _tempoMap;

            long absoluteTime = 0;
            foreach (MidiEvent midiEvent in track.Events)
            {
                absoluteTime += midiEvent.DeltaTime;
                string text = String.Empty;

                if (midiEvent.EventType == MidiEventType.Text)
                {
                    text = (midiEvent as TextEvent).Text;
                }
                if (midiEvent.EventType == MidiEventType.Lyric)
                {
                    text = (midiEvent as LyricEvent).Text;
                }

                if (!String.IsNullOrEmpty(text) && !text.StartsWith("["))
                {
                    try
                    {
                        lyrics.Add(absoluteTime, text);
                    }
                    catch (ArgumentException)
                    {
                        trackWarnings.AddTimed($"Lyric at the same time as another: {text} on {name}", absoluteTime, tempoMap);
                    }
                }
            }

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(VocalsTrackNotes), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", note.Time, tempoMap);
                        continue;
                    }
                }

                if (key == (byte)VocalsTrackNotes.LyricsPhrase1 || key == (byte)VocalsTrackNotes.LyricsPhrase2)
                {
                    phrases.Add((note.Time, note.Time + note.Length));
                    continue;
                }

                if (key < 36 || key > 84)
                {
                    // Include the percussion notes
                    if (!(key == 96 || key == 97))
                    {
                        continue;
                    }
                }

                var noteTime = note.Time;
                string text = String.Empty;
                if (lyrics.ContainsKey(noteTime))
                {
                    text = lyrics[noteTime];
                    lyrics.Remove(noteTime);
                }

                notes.Add(new VocalsNote(key, note.Time, note.Length, text));
            }
        }

        public Warnings RunChecks(Options parameters, List<(long, long)> extPhrases = null)
        {
            if (parameters.NoLyricAlignment)
            {
                trackWarnings.AddRange(MatchNoteLyrics());
            }
            if (parameters.NoLyricPhraseChecks)
            {
                trackWarnings.AddRange(CheckPhrases(extPhrases));
            }
            return trackWarnings;
        }

        public Warnings MatchNoteLyrics()
        {
            var warnings = new Warnings();

            if (lyrics.Count != 0)
            {
                foreach (long lyricTime in lyrics.Keys)
                {
                    string text = lyrics[lyricTime];
                    trackWarnings.AddTimed($"Lyric not aligned to a note: \"{text}\", {name}", lyricTime, tempoMap);
                }
            }

            foreach (VocalsNote note in notes)
            {
                if (note.Text == String.Empty)
                {
                    // Exclude percussion notes
                    if (note.Note == 96 || note.Note == 97)
                    {
                        continue;
                    }

                    trackWarnings.AddTimed($"Note without a lyric: {note.Note} on {name}", note.Position, tempoMap);
                }
            }

            return warnings;
        }

        public Warnings CheckPhrases(List<(long, long)> extPhrases = null)
        {
            var warnings = new Warnings();

            if (extPhrases == null)
            {
                // Use the track's own phrases instead
                extPhrases = phrases;
            }

            foreach (VocalsNote note in notes)
            {
                var noteStart = note.Position;
                var noteEnd = note.Position + note.Length;
                bool inPhrase = false;
                foreach ((long, long) times in extPhrases)
                {
                    var phraseStart = times.Item1;
                    var phraseEnd = times.Item2;
                    if (noteStart >= times.Item1 && noteStart <= times.Item2 && noteEnd <= times.Item2)
                    {
                        inPhrase = true;
                        break;
                    }
                }
                if (!inPhrase)
                {
                    trackWarnings.AddTimed($"Note outside of a phrase: {note.Note}, {name}", note.Position, tempoMap);
                }
            }

            return warnings;
        }
    }
}
