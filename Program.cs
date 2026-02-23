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

        CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // prevent abrupt termination
            "Exiting...".Write(ConsoleColor.Yellow);
            Environment.Exit(0);
        };

        var inputs = args;
        var running = true;

        while (running)
        {
            "cleantemp > ".Write(ConsoleColor.Cyan);
            var input = inputs.Length > 0 ? inputs[0] : ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                "Blank input".Write(ConsoleColor.Yellow);
                inputs = [];
                continue;
            }

            var command = input.ToLower();
            DirectoryInfo userTemp = new(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath());
            DirectoryInfo windowsTemp = new(@"C:\Windows\Temp");
            DirectoryInfo prefetch = new(@"C:\Windows\Prefetch");

            var prefetchSuccess = true;

            switch (command)
            {
                case "full":
                case "f":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    CleanFolder(prefetch, "Prefetch");
                    EmptyRecycleBin();
                    break;

                case "temp":
                case "t":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    break;

                case "prefetch":
                case "p":
                    CleanFolder(prefetch, "Prefetch");
                    break;

                case "credits":
                case "c":
                    "by cinax - https://github.com/cinaxdev/cleantemp".Write(ConsoleColor.Blue); // please do not edit credits
                    break;

                case "exit":
                case "quit":
                case "q":
                    running = false;
                    break;

                case "help":
                case "h":
                case "?":
                    """
                    Available commands:
                      (f)ull      - Clean User Temp, Windows Temp, Prefetch and empty Recycle Bin
                      (t)emp      - Clean User Temp and Windows Temp folders
                      (p)refetch  - Clean Prefetch folder
                      (h)elp      - Show this help message
                      (c)redits   - Show credits
                      (q)uit      - Exit the program
                    """.Write(ConsoleColor.Cyan);
                    prefetchSuccess = false;
                    break;

                default:
                    "Invalid command! Type (h)elp for commands".Write(ConsoleColor.Red);
                    prefetchSuccess = false;
                    break;
            }

            if (prefetchSuccess || command == "temp")
            {
                "Done, thanks for using!".Write(ConsoleColor.Green);
            }

            inputs = [];
        }
    }

    private static void CleanFolder(DirectoryInfo directory, string displayName)
    {
        if (!directory.Exists)
        {
            $"{displayName} folder doesnt exist: {directory.FullName}".Write(ConsoleColor.Yellow);
            return;
        }

        var success = true;

        try
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                try { file.Delete(); } catch { success = false; }
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                CleanFolderRecursive(dir);
                try { dir.Delete(); } catch { success = false; }
            }

            if (success)
                $"{displayName} cleaned: {directory.FullName}".Write(ConsoleColor.Green);
            else
                $"{displayName} could not be completely cleaned (Cannot delete some files): {directory.FullName}".Write(ConsoleColor.DarkYellow);
        }
        catch
        {
            $"{displayName} could not be cleaned: {directory.FullName}".Write(ConsoleColor.Red);
        }
    }

    private static void CleanFolderRecursive(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            $"{directory.FullName} doesnt exist".Write(ConsoleColor.Yellow);
            return;
        }
        var success = true;
        try
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                try { file.Delete(); } catch { success = false; }
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                CleanFolderRecursive(dir);
                try { dir.Delete(); } catch { success = false; }
            }
            if (success)
                $"{directory.FullName} cleaned".Write(ConsoleColor.Green);
            else
                $"{directory.FullName} could not be completely cleaned (Cannot delete some files)".Write(ConsoleColor.DarkYellow);
        }
        catch
        {
            $"{directory.FullName} could not be cleaned".Write(ConsoleColor.Red);
        }
    }

    private static void EmptyRecycleBin()
    {
        var shell = new Shell();
        Folder recycleBin = shell.NameSpace(10);
        if (recycleBin == null)
            return;

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
            UseShellExecute = true,
        };

        try
        {
            Process.Start(startInfo);
        }
        catch { }

        Environment.Exit(0);
    }
}
