using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

class StellarInstaller
{
    static readonly string ProductName = "Stellar OpenUTAU Pro";
    static readonly string Version = "1.0.0";
    static readonly string DefaultInstallDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProductName);

    static void Main(string[] args)
    {
        Console.Title = $"{ProductName} v{Version} Setup";
        Console.WriteLine();
        Console.WriteLine($"  ╔══════════════════════════════════════════╗");
        Console.WriteLine($"  ║  {ProductName} v{Version}      ║");
        Console.WriteLine($"  ║  github.com/stellartraveler5162-SLCG    ║");
        Console.WriteLine($"  ╚══════════════════════════════════════════╝");
        Console.WriteLine();

        string installDir = DefaultInstallDir;

        if (args.Length > 0)
        {
            installDir = args[0];
            Console.WriteLine($"  ├─ 命令参数指定路径: {installDir}");
        }
        else
        {
            Console.WriteLine($"  ├─ 默认安装路径 (无需管理员权限):");
            Console.WriteLine($"  │  {DefaultInstallDir}");
            Console.WriteLine($"  │");
            Console.Write($"  └─ 按 Enter 确认，或输入自定义路径后按 Enter: ");

            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                installDir = input.Trim();
            }
            else
            {
                installDir = DefaultInstallDir;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"  ▶ 安装位置: {installDir}");
        Console.WriteLine();

        try
        {
            if (Directory.Exists(installDir))
            {
                Console.WriteLine("  ⟳ 移除旧版本...");
                Directory.Delete(installDir, true);
            }
            Directory.CreateDirectory(installDir);

            Console.WriteLine("  ⬇ 解压文件中...");
            ExtractPayload(installDir);

            Console.WriteLine("  ⚡ 添加快捷方式...");
            CreateShortcuts(installDir);

            Console.WriteLine("  ✓ 安装完成！");
            Console.WriteLine();

            var exe = Path.Combine(installDir, "OpenUtau.exe");
            if (File.Exists(exe))
            {
                Console.WriteLine("  ▶ 启动中...");
                Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = installDir });
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✕ 没有写入权限。请尝试其他路径（如 D:\\{ProductName}）。");
            Console.ResetColor();
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✕ 安装失败: {ex.Message}");
            Console.ResetColor();
            Console.ReadKey();
        }
    }

    static void ExtractPayload(string installDir)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("StellarInstaller.payload.zip");
        if (stream == null) throw new Exception("未找到程序数据。安装包可能已损坏。");

        var tmpZip = Path.GetTempFileName() + ".zip";
        using (var fs = new FileStream(tmpZip, FileMode.Create))
            stream.CopyTo(fs);

        ZipFile.ExtractToDirectory(tmpZip, installDir, true);
        File.Delete(tmpZip);
    }

    static void CreateShortcuts(string installDir)
    {
        var exe = Path.Combine(installDir, "OpenUtau.exe");
        if (!File.Exists(exe)) return;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            var startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            var programsDir = Path.Combine(startMenu, "Programs");
            Directory.CreateDirectory(programsDir);

            dynamic smShortcut = shell.GetType().InvokeMember("CreateShortcut",
                BindingFlags.InvokeMethod, null, shell,
                new object[] { Path.Combine(programsDir, ProductName + ".lnk") })!;
            smShortcut.TargetPath = exe;
            smShortcut.WorkingDirectory = installDir;
            smShortcut.Save();

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dynamic dtShortcut = shell.GetType().InvokeMember("CreateShortcut",
                BindingFlags.InvokeMethod, null, shell,
                new object[] { Path.Combine(desktop, ProductName + ".lnk") })!;
            dtShortcut.TargetPath = exe;
            dtShortcut.WorkingDirectory = installDir;
            dtShortcut.Save();
        }
        catch { }
    }
}
