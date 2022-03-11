namespace RipCheck
{
    struct ProGuitarNote : INote
    {
        public byte Note { get; }
        public ProGuitarFretNumber Number { get; }
        public long Position { get; }
        public long Length { get; }

        public ProGuitarNote(byte note, ProGuitarFretNumber number, long position, long length)
        {
            Note = note;
            Number = number;
            Position = position;
            Length = length;
        }
    }
}
