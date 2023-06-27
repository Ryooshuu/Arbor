using Arbor;

namespace SampleGame;

public static class Program
{
    public static void Main(string[] args)
    {
        using var window = new Window("lkjfdsljkdfs", 854, 480);
        window.Run(new SampleGame());
    }
}