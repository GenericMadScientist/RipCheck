namespace RipCheck
{
    struct ProKeysNote : INote
    {
        public byte Note { get; }
        public long Position { get; }
        public long Length { get; }

        public ProKeysNote(byte note, long position, long length)
        {
            Note = note;
            Position = position;
            Length = length;
        }
    }
}
