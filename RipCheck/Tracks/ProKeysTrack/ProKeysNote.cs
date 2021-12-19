namespace RipCheck
{
    struct ProKeysNote
    {
        public ProKeysFretColour Fret { get; }
        public long Position { get; }
        public long Length { get; }

        public ProKeysNote(ProKeysFretColour fret, long position, long length)
        {
            Fret = fret;
            Position = position;
            Length = length;
        }
    }
}
