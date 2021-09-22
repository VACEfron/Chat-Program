namespace ChatProgram
{
    class Program
    {
        static void Main(string[] args)
            => new ChatServer().StartAsync().GetAwaiter().GetResult();
    }
}
