using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace Restarter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private string GetVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return $"V{v.Major}.{v.Minor}" + (v.Build > 0 ? $".{v.Build}" : "");
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            versionLabel.Text = GetVersion();
            var args = Environment.GetCommandLineArgs();

            if (args.Length == 2 && int.TryParse(args[1], out int oldPid))
            {
                statusTextBox.AppendText($"Waiting for process {oldPid}..." + Environment.NewLine);
                Refresh();

                // Off the UI thread — the form stays painted and responsive throughout.
                await Task.Run(() =>
                {
                    try { Process.GetProcessById(oldPid).WaitForExit(10000); }
                    catch (ArgumentException) { /* already gone — normal path */ }

//                    Thread.Sleep(5000);   // port-release window
                });
            }
            else
            {
                statusTextBox.AppendText($"Pausing..." + Environment.NewLine);
                Refresh();
                await Task.Run(() => Thread.Sleep(2000));
            }

            LaunchUnityApp();
        }

        private async void LaunchUnityApp()
        {
            statusTextBox.AppendText($"Restarting \"Hearing Test Suite\"..." + Environment.NewLine);

            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // up one more level
            folder = Path.GetDirectoryName(folder);

            var htsPath = Path.Combine(folder, "Hearing Test Suite.exe");

            try
            {
                var process = Process.Start(htsPath);
                await WaitForWindowAsync(process, timeoutMs: 30_000)
                    .ContinueWith(_ => Close(), TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                statusTextBox.AppendText($"Error starting app: {Environment.NewLine}{ex.Message}");
            }

        }

        private async Task WaitForWindowAsync(Process process, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                process.Refresh();

                if (process.HasExited)
                    break; // game crashed immediately — just close

                if (process.MainWindowHandle != IntPtr.Zero)
                    break; // window is visible

                await Task.Delay(200);
            }
        }



    }
}
