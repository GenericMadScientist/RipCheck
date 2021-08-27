using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

namespace RipCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var di = new DirectoryInfo(args[0]);
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
            FileInfo[] files = di.GetFiles("*.mid", options);
            foreach (FileInfo midi in files)
            {
                CheckMid(midi.FullName);
            }
        }

        private static void CheckMid(string midiPath)
        {
            var settings = new ReadingSettings
            {
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore
            };
            var midiFile = MidiFile.Read(midiPath, settings);
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
