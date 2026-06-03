using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

class StellarInstaller
{
    static readonly string ProductName = "Stellar OpenUTAU Pro";
    static readonly string InstallDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), ProductName);

    static void Main()
    {
        Console.Title = $"{ProductName} v1.0.0 Setup";
        Console.WriteLine();
        Console.WriteLine($"  {ProductName} v1.0.0 Setup");
        Console.WriteLine($"  github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro");
        Console.WriteLine();
        Console.WriteLine($"  Installing to: {InstallDir}");
        Console.WriteLine();

        try
        {
            if (Directory.Exists(InstallDir))
            {
                Console.WriteLine("  Removing previous installation...");
                Directory.Delete(InstallDir, true);
            }
            Directory.CreateDirectory(InstallDir);

            Console.WriteLine("  Extracting files...");
            ExtractPayload();

            Console.WriteLine("  Creating shortcuts...");
            CreateShortcuts();

            Console.WriteLine("  Install complete!");
            Console.WriteLine();

            var exe = Path.Combine(InstallDir, "OpenUtau.exe");
            if (File.Exists(exe))
            {
                Console.WriteLine("  Starting...");
                Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = InstallDir });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ERROR: {ex.Message}");
            Console.WriteLine("  Please run as Administrator.");
            Console.ReadKey();
        }
    }

    static void ExtractPayload()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("StellarInstaller.payload.zip");
        if (stream == null) throw new Exception("Payload not found in installer.");

        var tmpZip = Path.GetTempFileName() + ".zip";
        using (var fs = new FileStream(tmpZip, FileMode.Create))
            stream.CopyTo(fs);

        ZipFile.ExtractToDirectory(tmpZip, InstallDir, true);
        File.Delete(tmpZip);
    }

    static void CreateShortcuts()
    {
        var exe = Path.Combine(InstallDir, "OpenUtau.exe");
        if (!File.Exists(exe)) return;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            var shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            var startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            var programsDir = Path.Combine(startMenu, "Programs");
            Directory.CreateDirectory(programsDir);

            dynamic smShortcut = shell.GetType().InvokeMember("CreateShortcut",
                BindingFlags.InvokeMethod, null, shell,
                new object[] { Path.Combine(programsDir, ProductName + ".lnk") })!;
            smShortcut.TargetPath = exe;
            smShortcut.WorkingDirectory = InstallDir;
            smShortcut.Save();

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dynamic dtShortcut = shell.GetType().InvokeMember("CreateShortcut",
                BindingFlags.InvokeMethod, null, shell,
                new object[] { Path.Combine(desktop, ProductName + ".lnk") })!;
            dtShortcut.TargetPath = exe;
            dtShortcut.WorkingDirectory = InstallDir;
            dtShortcut.Save();
        }
        catch { }
    }
}
