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
        }

        while (true)
        {
            "cleantemp > ".Write(ConsoleColor.Cyan);
            string? input = args.Length > 0 ? args[0] : ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                "Blank input".Write(ConsoleColor.Yellow);
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
                    "by cinax - https://github.com/cinaxdev/cleantemp".Write(ConsoleColor.Blue); // please do not edit credits
                    break;

                case "exit":
                    return;

                default:
                    "Invalid command! Available commands are cleantemp, fullclean and cleanprefetch".Write(ConsoleColor.Red);
                    prefetchSuccess = false;
                    break;
            }

            if (prefetchSuccess || command == "cleantemp")
            {
                "Done, thanks for using!".Write(ConsoleColor.Green); // kinda buggy, double check if you dont think it works
            }

            args = [];
        }
    }

    private static void CleanFolder(string path, string displayName)
    {
        if (!Directory.Exists(path))
        {
            $"{displayName} folder doesnt exist: {path}".Write(ConsoleColor.Yellow);
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
                $"{displayName} cleaned: {path}".Write(ConsoleColor.Green);
            else
                $"{displayName} could not be completely cleaned (Cannot delete some files): {path}".Write(ConsoleColor.DarkYellow);
        }
        catch
        {
            $"{displayName} could not be cleaned: {path}".Write(ConsoleColor.Red);
        }
    }

    private static void EmptyRecycleBin()
    {
        var shell = new Shell();
        Folder recycleBin = shell.NameSpace(10);
        foreach (FolderItem item in recycleBin.Items())
        {
            try
            {
                item.InvokeVerb("delete");
            }
            catch
            {
                $"Failed to delete: {item.Name}".Write(ConsoleColor.Red);
            }
        }

        "Recycle Bin emptied".Write(ConsoleColor.Green);
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
}
