using static System.Console;

namespace CleanTemp;

internal static class ConsoleExtensions
{
    internal static void Write(this string text, ConsoleColor color)
    {
        if (ForegroundColor != color)
        {
            var original = ForegroundColor;
            ForegroundColor = color;
            WriteLine(text);
            ForegroundColor = original;
        }
        else
        {
            WriteLine(text);
        }
    }
}