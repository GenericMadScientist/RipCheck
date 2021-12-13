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
                    //case "PART DRUMS":
                    //case "PART VOCALS":
                    //case "PART REAL_GUITAR":
                    //case "PART REAL_GUITAR_22":
                    //case "PART REAL_BASS":
                    //case "PART REAL_BASS_22":
                    //case "PART REAL_KEYS_X":
                    //case "PART REAL_KEYS_H":
                    //case "PART REAL_KEYS_M":
                    //case "PART REAL_KEYS_E":
                    //case "PART KEYS_ANIM_LH":
                    //case "PART KEYS_ANIM_RH":
                    //case "HARM1":
                    //case "HARM2":
                    //case "HARM3":
                    //case "EVENTS":
                    //case "VENUE":
                    //case "BEAT":
                    //case "T1 GEMS":
                    //case "ANIM":
                    //case "TRIGGERS":
                    //case "BAND_DRUM":
                    //case "BAND_BASS":
                    //case "BAND_SINGER":
                    //    break;
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

            warnings.AddRange(guitarPart?.CheckDisjointChords());
            warnings.AddRange(bassPart?.CheckDisjointChords());

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
