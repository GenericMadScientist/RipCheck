using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace RipCheck
{
    public class CHSong
    {
        private readonly int resolution = -1;
        private readonly GuitarTrack guitarPart;
        private readonly GuitarTrack bassPart;
        private readonly GuitarTrack keysPart;
        private readonly TempoMap tempoMap;

        public CHSong(MidiFile midi)
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
                        guitarPart = new GuitarTrack(track, "PART GUITAR");
                        break;
                    case "PART BASS":
                        bassPart = new GuitarTrack(track, "PART BASS");
                        break;
                    case "PART KEYS":
                        keysPart = new GuitarTrack(track, "PART KEYS");
                        break;
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

            warnings.AddRange(guitarPart?.CheckChordSnapping(tempoMap));
            warnings.AddRange(bassPart?.CheckChordSnapping(tempoMap));
            warnings.AddRange(keysPart?.CheckChordSnapping(tempoMap));

            warnings.AddRange(guitarPart?.CheckDisjointChords(tempoMap));
            warnings.AddRange(bassPart?.CheckDisjointChords(tempoMap));

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
