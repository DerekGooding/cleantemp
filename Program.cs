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

        string[] inputs = args;

        while (true)
        {
            "cleantemp > ".Write(ConsoleColor.Cyan);
            string? input = inputs.Length > 0 ? inputs[0] : ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                "Blank input".Write(ConsoleColor.Yellow);
                inputs = [];
                continue;
            }

            string command = input.ToLower();
            DirectoryInfo userTemp = new(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath());
            DirectoryInfo windowsTemp = new(@"C:\Windows\Temp");
            DirectoryInfo prefetch = new(@"C:\Windows\Prefetch");

            bool prefetchSuccess = true;

            switch (command)
            {
                case "fullclean":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    CleanFolder(prefetch, "Prefetch");
                    EmptyRecycleBin();
                    break;

                case "cleantemp":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    break;

                case "cleanprefetch":
                    CleanFolder(prefetch, "Prefetch");
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

        bool success = true;

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
        bool success = true;
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
