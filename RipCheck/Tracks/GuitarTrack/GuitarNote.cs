namespace RipCheck
{
    struct GuitarNote
    {
        public GuitarFretColour Fret { get; }
        public long Position { get; }
        public long Length { get; }

        public GuitarNote(GuitarFretColour fret, long position, long length)
        {
            Fret = fret;
            Position = position;
            Length = length;
        }
    }
}
