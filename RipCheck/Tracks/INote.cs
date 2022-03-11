namespace RipCheck
{
    public interface INote
    {
        byte Note { get; }
        long Position { get; }
        long Length { get; }
    }
}
