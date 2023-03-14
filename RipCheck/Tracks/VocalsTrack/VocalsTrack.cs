using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace RipCheck
{
    class VocalsTrack
    {
        private readonly List<INote> notes = new();
        private readonly Dictionary<long, string> lyrics = new();
        private readonly List<(long, long)> phrases = new();
        public List<(long, long)> Phrases { get { return phrases; } }
        private readonly TempoMap tempoMap;
        private readonly string name;
        public string Name { get { return name; } }

        private readonly Warnings trackWarnings = new Warnings();

        public VocalsTrack(TrackChunk track, TempoMap _tempoMap, string instrument, CheckOptions parameters)
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
                else if (midiEvent.EventType == MidiEventType.Lyric)
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

            foreach (Note midiNote in track.GetNotes())
            {
                byte key = midiNote.NoteNumber;
                var note = (VocalsTrackNote)key;

                if (parameters.UnknownNotes)
                {
                    if (!Enum.IsDefined(typeof(VocalsTrackNote), key))
                    {
                        trackWarnings.AddTimed($"Unknown note: {key} on {name}", midiNote.Time, tempoMap);
                        continue;
                    }
                }

                if (note == VocalsTrackNote.LyricsPhrase1 || note == VocalsTrackNote.LyricsPhrase2)
                {
                    phrases.Add((midiNote.Time, midiNote.Time + midiNote.Length));
                    continue;
                }

                if (note < VocalsTrackNote.C2 || note > VocalsTrackNote.C6)
                {
                    // Include the percussion notes
                    if (!(note == VocalsTrackNote.ShownPercussion || note == VocalsTrackNote.NotShownPercussion))
                    {
                        continue;
                    }
                }

                var noteTime = midiNote.Time;
                string text = String.Empty;
                if (lyrics.ContainsKey(noteTime))
                {
                    text = lyrics[noteTime];
                    lyrics.Remove(noteTime);
                }

                notes.Add(new VocalsNote(key, midiNote.Time, midiNote.Length, text));
            }
        }

        public Warnings RunChecks(List<(long, long)> extPhrases = null)
        {
            trackWarnings.AddRange(MatchNoteLyrics());
            trackWarnings.AddRange(CheckPhrases(extPhrases));
            trackWarnings.AddRange(CommonChecks.CheckOverlappingNotes(Difficulty.None, notes, name, tempoMap));
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
                    if (note.Note == (byte)VocalsTrackNote.ShownPercussion || note.Note == (byte)VocalsTrackNote.NotShownPercussion)
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
