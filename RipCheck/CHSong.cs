using Melanchall.DryWetMidi.Core;

namespace RipCheck
{
    public class CHSong
    {
        private readonly int resolution = -1;
        private readonly GuitarTrack guitarPart;
        private readonly GuitarTrack bassPart;
        private readonly GuitarTrack keysPart;

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

            warnings.AddRange(guitarPart?.CheckChordSnapping());
            warnings.AddRange(bassPart?.CheckChordSnapping());
            warnings.AddRange(keysPart?.CheckChordSnapping());

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
