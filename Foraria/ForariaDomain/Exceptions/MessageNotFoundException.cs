namespace ForariaDomain.Exceptions
{
    public class MessageNotFoundException : Exception
    {
        public MessageNotFoundException(string message) : base(message) { }
    }
}
