/*
https://github.com/cinaxdev/cleantemp/
do not edit anything except "ConsoleColor.COLORNAME" if you dont know what you're doing
i was tired while doing it, this may contain minor bugs
made just for fun and improving myself!
:D
*/

using System.Security.Principal;
using System.Diagnostics;
using Shell32;
using static System.Console;

namespace CleanTemp;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (!IsAdministrator())
        {
            RestartAsAdmin(args);
            return;
        }

        while (true)
        {
            ForegroundColor = ConsoleColor.Cyan; // you can change color of "cleantemp >" by editing "ConsoleColor.Cyan" (like ConsoleColor.Red or ConsoleColor.Yellow). you can also do this to other lines that has this.
            Write("cleantemp > ");
            ResetColor();
            string? input = args.Length > 0 ? args[0] : ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteColored("Blank input", ConsoleColor.Yellow);
                args = [];
                continue;
            }

            string command = input.ToLower();
            string userTemp = Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath();
            const string windowsTemp = @"C:\Windows\Temp";

            bool prefetchSuccess = true;

            switch (command)
            {
                case "fullclean":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    CleanFolder(@"C:\Windows\Prefetch", "Prefetch");
                    EmptyRecycleBin();
                    break;

                case "cleantemp":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    break;

                case "cleanprefetch":
                    CleanFolder(@"C:\Windows\Prefetch", "Prefetch");
                    break;

                case "credits":
                    WriteColored("by cinax - https://github.com/cinaxdev/cleantemp", ConsoleColor.Blue); // please do not edit credits
                    break;

                case "exit":
                    return;

                default:
                    WriteColored("Invalid command! Available commands are cleantemp, fullclean and cleanprefetch", ConsoleColor.Red);
                    prefetchSuccess = false;
                    break;
            }

            if (prefetchSuccess || command == "cleantemp")
            {
                WriteColored("Done, thanks for using!", ConsoleColor.Green); // kinda buggy, double check if you dont think it works
            }

            args = [];
        }
    }

    private static void CleanFolder(string path, string displayName)
    {
        if (!Directory.Exists(path))
        {
            WriteColored($"{displayName} folder doesnt exist: {path}", ConsoleColor.Yellow);
            return;
        }

        bool success = true;

        try
        {
            foreach (string file in Directory.GetFiles(path))
            {
                try { File.Delete(file); } catch { success = false; }
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                try { Directory.Delete(dir, true); } catch { success = false; }
            }

            if (success)
                WriteColored($"{displayName} cleaned: {path}", ConsoleColor.Green);
            else
                WriteColored($"{displayName} could not be completely cleaned (Cannot delete some files): {path}", ConsoleColor.DarkYellow);
        }
        catch
        {
            WriteColored($"{displayName} could not be cleaned: {path}", ConsoleColor.Red);
        }
    }

    private static void EmptyRecycleBin()
    {
        try
        {
            var shell = new Shell();
            Folder recycleBin = shell.NameSpace(10);
            foreach (FolderItem item in recycleBin.Items())
            {
                item.InvokeVerb("delete");
            }

            WriteColored("Recycle Bin emptied", ConsoleColor.Green);
        }
        catch
        {
            WriteColored("Cannot empty recycle bin", ConsoleColor.Red);
        }
    }

    private static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void RestartAsAdmin(string[] args)
    {
        var exeName = Environment.ProcessPath;
        var startInfo = new ProcessStartInfo
        {
            FileName = exeName,
            Verb = "runas",
            Arguments = string.Join(" ", args),
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch { }

        Environment.Exit(0);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = ForegroundColor;
        ForegroundColor = color;
        WriteLine(text);
        ForegroundColor = original;
    }
}