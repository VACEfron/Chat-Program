namespace Client
{
    class Program
    {
        static void Main(string[] args)
            => new Client().RunClientAsync().GetAwaiter().GetResult();
    }
}
