using System;
using wraithspire.engine;

namespace Wraithspire
{
    internal static class Program
    {
        private static void Main()
        {
            using var window = new Window(1280, 720, "Wraithspire Engine");
            window.Run();
        }
    }

}
