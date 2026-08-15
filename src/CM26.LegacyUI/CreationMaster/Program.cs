using System;
using System.IO;
using System.Windows.Forms;

namespace CreationMaster;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		Fc26HostBridge.Configure(args);
		FifaLibrary.FifaEnvironment.Fc26AssetExporter = Fc26HostBridge.ExportAsset;
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-plan", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var count = Fc26SnapshotLoader.WriteChanges(args[2]);
				Environment.ExitCode = count == 0 ? 0 : 2;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-smoke", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				using var main = new MainForm();
				main.Show();
				Application.DoEvents();
				main.LoadFc26Snapshot(args[1], showCountry: false);
				foreach (var section in args[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					main.ShowFc26Section(section.Trim());
					Application.DoEvents();
				}
				main.Dispose();
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.Exit(1);
			}
		}
		var diagnostic = args.Length >= 1 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase);
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		Application.ThreadException += (_, e) => HandleUnhandled(e.Exception, diagnostic);
		AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleUnhandled(e.ExceptionObject as Exception, diagnostic);
		try
		{
			var main = new MainForm();
			if (args.Length >= 2 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase))
			{
				main.LoadFc26Snapshot(args[1], showCountry: args.Length < 3);
				if (args.Length >= 3) main.ShowFc26Section(args[2]);
			}
			Application.Run(main);
		}
		catch (Exception ex)
		{
			var log = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			File.WriteAllText(log, ex.ToString());
			if (!(args.Length >= 1 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase)))
				MessageBox.Show(ex.Message + "\r\n\r\n" + log, "Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private static void HandleUnhandled(Exception exception, bool diagnostic)
	{
		var log = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
		File.WriteAllText(log, (exception ?? new Exception("Unknown legacy UI error.")).ToString());
		if (diagnostic) Environment.Exit(1);
		MessageBox.Show((exception?.Message ?? "Unknown error") + "\r\n\r\n" + log,
			"Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}
