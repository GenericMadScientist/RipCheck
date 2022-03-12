using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

namespace RipCheck
{
    public class CHSong
    {
        private readonly int resolution = -1;
        private readonly TempoMap tempoMap;
        private readonly Dictionary<string, GuitarTrack> guitarTracks = new();
        private readonly DrumsTrack drumTrack;
        private readonly Dictionary<string, VocalsTrack> vocalsTracks = new();
        private readonly Dictionary<string, ProGuitarTrack> proGuitarTracks = new();
        private readonly Dictionary<string, ProKeysTrack> proKeysTracks = new();

        public CHSong(MidiFile midi, CheckOptions parameters)
        {
            var timeDivision = midi.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (timeDivision != null)
            {
                resolution = timeDivision.TicksPerQuarterNote;
            }

            tempoMap = midi.GetTempoMap();

            foreach (TrackChunk track in midi.GetTrackChunks())
            {
                string name = TrackName(track);
                switch (name)
                {
                    case "PART GUITAR":
                    case "PART GUITAR COOP":
                    case "PART RHYTHM":
                    case "PART BASS":
                    case "PART KEYS":
                    case "T1 GEMS":
                        guitarTracks.Add(name, new GuitarTrack(track, tempoMap, name, parameters));
                        break;
                    case "PART DRUMS":
                        drumTrack = new DrumsTrack(track, tempoMap, name, parameters);
                        break;
                    case "PART VOCALS":
                    case "HARM1":
                    case "HARM2":
                    case "HARM3":
                        vocalsTracks.Add(name, new VocalsTrack(track, tempoMap, name, parameters));
                        break;
                    case "PART REAL_GUITAR":
                    case "PART REAL_GUITAR_22":
                    case "PART REAL_BASS":
                    case "PART REAL_BASS_22":
                        proGuitarTracks.Add(name, new ProGuitarTrack(track, tempoMap, name, parameters));
                        break;
                    case "PART REAL_KEYS_X":
                    case "PART REAL_KEYS_H":
                    case "PART REAL_KEYS_M":
                    case "PART REAL_KEYS_E":
                        proKeysTracks.Add(name, new ProKeysTrack(track, tempoMap, name, parameters));
                        break;
                }
            }
        }

        public Warnings RunChecks(CheckOptions parameters)
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

            foreach (GuitarTrack track in guitarTracks.Values)
            {
                warnings.AddRange(track.RunChecks(parameters));
            }

            warnings.AddRange(drumTrack?.RunChecks());
            
            if (parameters.Vocals)
            {
                foreach (VocalsTrack track in vocalsTracks.Values)
                {
                    if (track.Name == "HARM3")
                    {
                        // HARM3 does not have phrase markers, so we have to use HARM2's phrases instead
                        if (!vocalsTracks.ContainsKey("HARM2"))
                        {
                            warnings.Add("Track HARM3 present without track HARM2, skipping it");
                            continue;
                        }

                        warnings.AddRange(track.RunChecks(vocalsTracks["HARM2"].Phrases));
                    }
                    else
                    {
                        warnings.AddRange(track.RunChecks());
                    }
                }
            }

            if (parameters.ProTracks)
            {
                foreach (ProGuitarTrack track in proGuitarTracks.Values)
                {
                    warnings.AddRange(track.RunChecks(parameters));
                }
                
                foreach (ProKeysTrack track in proKeysTracks.Values)
                {
                    warnings.AddRange(track.RunChecks());
                }
            }

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
