using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;

namespace RipCheck
{
    public class CHSong
    {
        private readonly int resolution = -1;
        private readonly Dictionary<string, GuitarTrack> guitarTracks = new();
        // private readonly DrumTrack drumTrack = new();
        // private readonly Dictionary<string, VocalsTrack> vocalsTracks = new();
        // private readonly Dictionary<string, ProGuitarTrack> proGuitarTracks = new();
        // private readonly Dictionary<string, ProKeysTrack> proKeysTracks = new();

        public CHSong(MidiFile midi)
        {
            var timeDivision = midi.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (timeDivision != null)
            {
                resolution = timeDivision.TicksPerQuarterNote;
            }

            foreach (TrackChunk track in midi.GetTrackChunks())
            {
                string name = TrackName(track);
                switch (name)
                {
                    case "PART GUITAR":
                    case "PART BASS":
                    case "PART KEYS":
                    //case "T1 GEMS":
                        guitarTracks.Add(name, new GuitarTrack(track, name));
                        break;
                    //case "PART DRUMS":
                        // drumTrack = new DrumTrack(track, name));
                        // break;
                    //case "PART VOCALS":
                    //case "HARM1":
                    //case "HARM2":
                    //case "HARM3":
                        // vocalsTracks.Add(name, new VocalsTrack(track, name));
                        // break;
                    //case "PART REAL_GUITAR":
                    //case "PART REAL_GUITAR_22":
                    //case "PART REAL_BASS":
                    //case "PART REAL_BASS_22":
                        // proGuitarTracks.Add(name, new ProGuitarTrack(track, name));
                        // break;
                    //case "PART REAL_KEYS_X":
                    //case "PART REAL_KEYS_H":
                    //case "PART REAL_KEYS_M":
                    //case "PART REAL_KEYS_E":
                        // proKeysTracks.Add(name, new ProKeysTrack(track, name));
                        // break;
                    //case "PART KEYS_ANIM_LH":
                    //case "PART KEYS_ANIM_RH":
                    //case "EVENTS":
                    //case "VENUE":
                    //case "BEAT":
                    //case "ANIM":
                    //case "TRIGGERS":
                    //case "BAND_DRUM":
                    //case "BAND_BASS":
                    //case "BAND_SINGER":
                        // break;
                }
            }
        }

        public Warnings RunChecks()
        {
            var warnings = new Warnings();
            if (resolution == -1)
            {
                warnings.Add("Midi uses SMPTE time");
            }
            else if (resolution != 480)
            {
                warnings.Add($"Midi has resolution {resolution}");
            }

            foreach (string name in guitarTracks.Keys)
            {
                if (guitarTracks.ContainsKey(name))
                {
                    guitarTracks[name].CheckChordSnapping();
                    guitarTracks[name].CheckUnknownNotes();
                    if (name != "PART KEYS")
                    {
                        guitarTracks[name].CheckDisjointChords();
                    }
                }
            }

            // drumTrack?.CheckUnknownNotes();
            
            // foreach (string name in vocalsTracks.Keys)
            // {
            //     if (vocalsTracks.ContainsKey(name))
            //     {
            //         vocalsTracks[name].CheckSomething();
            //     }
            // }
            
            // foreach (string name in proGuitarTracks.Keys)
            // {
            //     if (proGuitarTracks.ContainsKey(name))
            //     {
            //         proGuitarTracks[name].CheckSomething();
            //     }
            // }
            
            // foreach (string name in proKeysTracks.Keys)
            // {
            //     if (proKeysTracks.ContainsKey(name))
            //     {
            //         proKeysTracks[name].CheckSomething();
            //     }
            // }

            return warnings;
        }

        private static string TrackName(TrackChunk track)
        {
            foreach (MidiEvent e in track.Events)
            {
                if (e.EventType == MidiEventType.SequenceTrackName)
                {
                    return ((SequenceTrackNameEvent)e).Text;
                }
            }

            return "";
        }
    }
}
