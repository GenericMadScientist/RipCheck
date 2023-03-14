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
                    case TrackNames.Guitar:
                    case TrackNames.GuitarCoop:
                    case TrackNames.Rhythm:
                    case TrackNames.Bass:
                    case TrackNames.Keys:
                    case TrackNames.GH1Guitar:
                        guitarTracks.Add(name, new GuitarTrack(track, tempoMap, name, parameters));
                        break;
                    case TrackNames.Drums:
                        drumTrack = new DrumsTrack(track, tempoMap, name, parameters);
                        break;
                    case TrackNames.Vocals:
                    case TrackNames.Harmony1:
                    case TrackNames.Harmony2:
                    case TrackNames.Harmony3:
                        vocalsTracks.Add(name, new VocalsTrack(track, tempoMap, name, parameters));
                        break;
                    case TrackNames.RealGuitar:
                    case TrackNames.RealGuitar22:
                    case TrackNames.RealBass:
                    case TrackNames.RealBass22:
                        proGuitarTracks.Add(name, new ProGuitarTrack(track, tempoMap, name, parameters));
                        break;
                    case TrackNames.RealKeysExpert:
                    case TrackNames.RealKeysHard:
                    case TrackNames.RealKeysMedium:
                    case TrackNames.RealKeysEasy:
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
                    if (track.Name == TrackNames.Harmony3)
                    {
                        // HARM3 does not have phrase markers, so we have to use HARM2's phrases instead
                        if (!vocalsTracks.ContainsKey(TrackNames.Harmony2))
                        {
                            warnings.Add("Track HARM3 present without track HARM2, skipping it");
                            continue;
                        }

                        warnings.AddRange(track.RunChecks(vocalsTracks[TrackNames.Harmony2].Phrases));
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
