using CommandLine;
using Melanchall.DryWetMidi.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace RipCheck
{
    public class Options
    {
        [Option('n', "notesonly", Required = false, HelpText = "Only scan files called \"notes.mid\".")]
        public bool NotesOnly { get; set; }

        [Option('a', "checkall", Required = false, HelpText = "Enables checking of Pro instruments and unknown notes. Overrides -g and -r.")]
        public bool CheckAll { get; set; }

        [Option('r', "rbpro", Required = false, HelpText = "Enables checking of Pro instruments.")]
        public bool RBPro { get; set; }

        [Option('g', "ghband", Required = false, HelpText = "Excludes the disjoint chord check for GHWT and onward songs.")]
        public bool GHBand { get; set; }

        [Option('v', "vocals", Required = false, HelpText = "Enables checking the vocals track for issues.")]
        public bool Vocals { get; set; }

        [Value(0, HelpText = "Directory to check the charts of.")]
        public string Directory { get; set; }
    }

    public class CheckOptions
    {
        /// <summary>
        /// Check for unrecognized notes and Pro Guitar fret numbers/channels.
        /// </summary>
        public bool UnknownNotes { get; set; }

        /// <summary>
        /// Check for disjoint chords on 5-fret tracks.
        /// </summary>
        public bool Disjoints { get; set; }

        /// <summary>
        /// Check Pro Guitar/Keys for issues.
        /// </summary>
        public bool ProTracks { get; set; }

        /// <summary>
        /// Check Vocals for issues.
        /// </summary>
        public bool Vocals { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CheckOptions parameters = new()
            {
                UnknownNotes = false,
                Disjoints = true,
                ProTracks = false,
                Vocals = false
            };
            bool notesOnly = false;
            string directory = "";
            bool earlyReturn = false;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    notesOnly = o.NotesOnly;
                    directory = o.Directory;

                    if (o.CheckAll)
                    {
                        parameters.UnknownNotes = true;
                        parameters.ProTracks = true;
                        parameters.Vocals = true;
                    }
                    else
                    {
                        if (o.RBPro)
                        {
                            parameters.ProTracks = true;
                        }
                        if (o.GHBand)
                        {
                            parameters.Disjoints = false;
                        }
                        parameters.Vocals = o.Vocals;
                    }

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

            Console.WriteLine($"Checking directory {directory}");
            Console.WriteLine(); // Put a line break between the starting message and the warnings

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

            Console.WriteLine($"Done.");
        }

        private static void CheckMid(string midiPath, CheckOptions parameters)
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
