namespace RipCheck
{
    struct GuitarNote : INote
    {
        public byte Note { get; }
        public long Position { get; }
        public long Length { get; }

        public GuitarNote(byte fret, long position, long length)
        {
            Note = fret;
            Position = position;
            Length = length;
        }
    }
}
