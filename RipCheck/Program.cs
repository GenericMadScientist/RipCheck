using CommandLine;
using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

namespace RipCheck
{
    class Program
    {
        public class Options
        {
            [Option(Required = false, HelpText = "Only scan files called notes.mid.")]
            public bool NotesOnly { get; set; }

            [Value(0, HelpText = "Directory containing the .mid files.")]
            public string Directory { get; set; }
        }

        static void Main(string[] args)
        {
            bool notesOnly = false;
            string directory = "";
            bool earlyReturn = false;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
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
                Console.WriteLine($"Directory {args[0]} does not exist");
                return;
            }

            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false
            };
            string search = notesOnly ? "notes.mid" : "*.mid";
            FileInfo[] files = di.GetFiles(search, options);
            foreach (FileInfo midi in files)
            {
                CheckMid(midi.FullName);
            }
        }

        private static void CheckMid(string midiPath)
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

            var song = new CHSong(midiFile);
            Warnings warnings = song.RunChecks();
            if (!warnings.IsEmpty())
            {
                Console.WriteLine($"File {midiPath} has problems");
                warnings.PrintWarnings();
            }
        }
    }
}
