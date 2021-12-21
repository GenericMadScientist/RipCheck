namespace RipCheck
{
    struct ProGuitarNote
    {
        public ProGuitarFretColour Fret { get; }
        public ProGuitarFretNumber Number { get; }
        public long Position { get; }
        public long Length { get; }

        public ProGuitarNote(ProGuitarFretColour fret, ProGuitarFretNumber number, long position, long length)
        {
            Fret = fret;
            Number = number;
            Position = position;
            Length = length;
        }
    }
}
