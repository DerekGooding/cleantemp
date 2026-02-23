/*
https://github.com/cinaxdev/cleantemp/
do not edit anything except "ConsoleColor.COLORNAME" if you dont know what you're doing
i was tired while doing it, this may contain minor bugs
made just for fun and improving myself!
:D
*/

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static System.Console;

namespace CleanTemp;

internal static partial class Program
{
    [LibraryImport("Shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);
    // dwFlags = 0x00000007 to suppress all UI and confirmation

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

        DirectoryInfo userTemp = new(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath());
        DirectoryInfo windowsTemp = new(@"C:\Windows\Temp");
        DirectoryInfo prefetch = new(@"C:\Windows\Prefetch");

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

    private static void CleanFolder(DirectoryInfo directory, string displayName = "")
    {
        if (!directory.Exists)
        {
            $"{displayName} folder doesnt exist: {directory.FullName}".Write(ConsoleColor.Yellow);
            return;
        }

        var success = true;

        try
        {
            foreach (var file in directory.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                try { file.Delete(); } catch { success = false; }
            }

            foreach (var dir in directory.GetDirectories())
            {
                CleanFolder(dir);
                if (displayName?.Length == 0)
                {
                    try { dir.Delete(); } catch { success = false; }
                }
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

    private static void EmptyRecycleBin()
    {
        const uint SHERB_NOCONFIRMATION = 0x00000001;
        const uint SHERB_NOPROGRESSUI = 0x00000002;
        const uint SHERB_NOSOUND = 0x00000004;

        var result = SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

        if (result == 0)
            "Recycle Bin emptied".Write(ConsoleColor.Green);
        else
            $"Recycle Bin could not be emptied (HRESULT: {result})".Write(ConsoleColor.Red);
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
