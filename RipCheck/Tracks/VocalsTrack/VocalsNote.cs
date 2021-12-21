namespace RipCheck
{
    struct VocalsNote
    {
        public byte Note { get; }
        public long Position { get; }
        public long Length { get; }
        public string Text { get; }

        public VocalsNote(byte note, long position, long length, string text)
        {
            Note = note;
            Position = position;
            Length = length;
            Text = text;
        }
    }
}
