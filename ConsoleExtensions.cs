/*
https://github.com/cinaxdev/cleantemp/
do not edit anything except "ConsoleColor.COLORNAME" if you dont know what you're doing
i was tired while doing it, this may contain minor bugs
made just for fun and improving myself!
:D
*/

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