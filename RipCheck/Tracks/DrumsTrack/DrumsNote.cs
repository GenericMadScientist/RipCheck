namespace RipCheck
{
    struct DrumsNote : INote
    {
        public byte Note { get; }
        public long Position { get; }
        public long Length { get; }

        public DrumsNote(byte note, long position, long length)
        {
            Note = note;
            Position = position;
            Length = length;
        }
    }
}
