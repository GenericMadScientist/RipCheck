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
        private readonly Dictionary<long, TextEvent> lyrics = new();
        private readonly TempoMap tempoMap;
        private readonly string name;

        private Warnings trackWarnings = new Warnings();

        public VocalsTrack(TrackChunk track, TempoMap _tempoMap, string instrument)
        {
            name = instrument;
            tempoMap = _tempoMap;

            long absoluteTime = 0;
            foreach (MidiEvent midiEvent in track.Events)
            {
                absoluteTime += midiEvent.DeltaTime;

                if (midiEvent.EventType == MidiEventType.Text)
                {
                    TextEvent textEvent = midiEvent as TextEvent;
                    if (!textEvent.Text.StartsWith("["))
                    {
                        lyrics.Add(absoluteTime, (midiEvent as TextEvent));
                    }
                }
            }

            foreach (Note note in track.GetNotes())
            {
                byte key = note.NoteNumber;

                if (!Enum.IsDefined(typeof(VocalsTrackNotes), key))
                {
                    var position = note.Time;
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    trackWarnings.Add($"Unknown note: {key} on {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                    continue;
                }

                if (key < 36 || key > 84)
                {
                    continue;
                }

                var noteTime = note.Time;
                string text = String.Empty;
                if (lyrics.ContainsKey(noteTime))
                {
                    text = lyrics[noteTime].Text;
                    lyrics.Remove(noteTime);
                }

                notes.Add(new VocalsNote(key, note.Time, note.Length, text));
            }
        }

        public Warnings RunChecks()
        {
            // TODO:
            // Check that that every note is within a phrase
            trackWarnings.AddRange(MatchNoteLyrics());
            return trackWarnings;
        }

        public Warnings MatchNoteLyrics()
        {
            var warnings = new Warnings();

            if (lyrics.Count != 0)
            {
                foreach (long lyricTime in lyrics.Keys)
                {
                    string text = lyrics[lyricTime].Text;
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(lyricTime, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(lyricTime, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    trackWarnings.Add($"Lyric not aligned to a note: {text} on {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                }
            }

            foreach (VocalsNote note in notes)
            {
                if (note.Text == String.Empty)
                {
                    var position = note.Position;
                    var time = (MetricTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.Metric, tempoMap);
                    var ticks = (BarBeatTicksTimeSpan) TimeConverter.ConvertTo(position, TimeSpanType.BarBeatTicks, tempoMap);
                    int minutes = 60 * time.Hours + time.Minutes;
                    int seconds = time.Seconds;
                    int millisecs = time.Milliseconds;
                    trackWarnings.Add($"Note without a lyric: {note.Note} on {name} at {minutes}:{seconds:d2}.{millisecs:d3} (MBT: {ticks.Bars}.{ticks.Beats}.{ticks.Ticks})");
                }
            }

            return warnings;
        }
    }
}
