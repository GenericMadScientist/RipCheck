namespace RipCheck
{
    struct DrumsNote
    {
        public DrumsFretColour Fret { get; }
        public long Position { get; }
        public long Length { get; }

        public DrumsNote(DrumsFretColour fret, long position, long length)
        {
            Fret = fret;
            Position = position;
            Length = length;
        }
    }
}
