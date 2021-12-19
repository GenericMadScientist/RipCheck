using CommandLine;
using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

namespace RipCheck
{
    public class Options
    {
        [Option('n', "notesonly", Required = false, HelpText = "Only scan files called \"notes.mid\".")]
        public bool NotesOnly { get; set; }

        [Option('u', "unknownnotes", Required = false, HelpText = "Check for unrecognized notes and Pro Guitar fret numbers/channels.")]
        public bool UnknownNotes { get; set; }

        [Option('d', "nodisjoint", Required = false, HelpText = "Don't check for disjoint chords.")]
        public bool NoDisjoints { get; set; }

        [Option('s', "nochordsnapping", Required = false, HelpText = "Don't check for chord snapping issues.")]
        public bool NoChordSnapping { get; set; }

        [Option('l', "nolyricalignment", Required = false, HelpText = "Don't check for lyric alignment issues.")]
        public bool NoLyricAlignment { get; set; }

        [Option('p', "nolyricphrasechecks", Required = false, HelpText = "Don't check for vocals notes outside of phrases.")]
        public bool NoLyricPhraseChecks { get; set; }

        [Value(0, HelpText = "Directory to check the charts of.")]
        public string Directory { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Options parameters = new();
            bool notesOnly = false;
            string directory = "";
            bool earlyReturn = false;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    parameters = o;
                    notesOnly = o.NotesOnly;
                    directory = o.Directory;
                    if (directory is null)
                    {
                        earlyReturn = true;
                        Console.WriteLine("Please specify the directory");
                    }
                })
                .WithNotParsed(_ =>
                {
                    earlyReturn = true;
                });

            if (earlyReturn)
            {
                return;
            }

            var di = new DirectoryInfo(directory);
            if (!di.Exists)
            {
                Console.WriteLine($"Directory {directory} does not exist");
                return;
            }

            var fileOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false
            };
            string search = notesOnly ? "notes.mid" : "*.mid";
            FileInfo[] files = di.GetFiles(search, fileOptions);
            foreach (FileInfo midi in files)
            {
                Debug.WriteLine($"Checking {midi.FullName}");
                CheckMid(midi.FullName, parameters);
            }
        }

        private static void CheckMid(string midiPath, Options parameters)
        {
            var settings = new ReadingSettings
            {
                InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore
            };

            MidiFile midiFile;
            try
            {
                midiFile = MidiFile.Read(midiPath, settings);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error parsing {midiPath}");
                return;
            }

            var song = new CHSong(midiFile, parameters);
            Warnings warnings = song.RunChecks(parameters);
            if (!warnings.IsEmpty())
            {
                Console.WriteLine($"File {midiPath} has problems");
                warnings.PrintWarnings();
                Console.WriteLine(); // Put a line break between each file's warnings
            }
        }
    }
}
