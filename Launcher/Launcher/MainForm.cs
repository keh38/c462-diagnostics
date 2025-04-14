using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Serilog;

using CoreAudio;
using CoreAudio.Interfaces;

using D128NET;
using KLib;

namespace Launcher
{
    public partial class MainForm : Form
    {
        bool _configButtonPressed = false;
        bool _launchStarted = false;
        Timer _timer;
        int _delayTime = 5000;

        HardwareConfiguration _config;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            versionLabel.Text = GetVersion();
            statusTextBox.Text = "Starting..." + Environment.NewLine;

            _timer = new Timer();
            _timer.Interval = _delayTime;
            _timer.Tick += Timer_Tick;
            _timer.Enabled = true;

            if (!Directory.Exists(FileLocations.RootFolder))
            {
                Directory.CreateDirectory(FileLocations.RootFolder);
            }
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await StartLogging();
            Log.Information($"Started {GetVersion()}");
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !_launchStarted)
            {
                LaunchUnityApp();
            }
            else if ((e.KeyCode == Keys.F2 || e.KeyCode == Keys.C) && !_configButtonPressed)
            {
                _timer.Enabled = false;
                _configButtonPressed = true;
                Log.Information("Config button pressed");
                ShowConfigDialog();
                _configButtonPressed = false;
                LaunchUnityApp();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void ShowConfigDialog()
        {
            var dlg = new ConfigForm();
            dlg.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("Exit");
            Log.CloseAndFlush();
        }

        private string GetVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return $"V{v.Major}.{v.Minor}" + (v.Build > 0 ? $".{ v.Build}" : "");
        }

        private async Task StartLogging()
        {
            var logLevel = new Serilog.Core.LoggingLevelSwitch();
            logLevel.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevel)
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(FileLocations.RootFolder, "Logs", "HTSLauncher-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    flushToDiskInterval: TimeSpan.FromSeconds(30),
                    buffered: true)
                .CreateLogger();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            if (!_configButtonPressed && !_launchStarted)
            {
                LaunchUnityApp();
            }
        }

        private void LaunchUnityApp()
        {
            _launchStarted = true;

            string errorMsg = "";
            try
            {
                errorMsg = ValidateHardwareSetup();
            }
            catch (Exception ex)
            {
                errorMsg = "Failed to initialize hardware";
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            if (string.IsNullOrEmpty(errorMsg))
            {
                statusTextBox.AppendText("Starting Hearing Test Suite..." + Environment.NewLine);
                Log.Information("Starting HTS");
                System.Threading.Thread.Sleep(1000);

                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
                var index = folder.IndexOf("Launcher");
                folder = Path.Combine(folder.Substring(0, index - 1), "Diagnostics", "Build");
#else
                // up one more level
                folder = Path.GetDirectoryName(folder);
#endif
                var htsPath = Path.Combine(folder, "Hearing Test Suite.exe");

                try
                {
                    Log.Information($"App path = {htsPath}");
                    Process.Start(htsPath);
                }
                catch (Exception ex)
                {
                    errorMsg = "Error starting app";
                    Log.Error($"Error starting app:{Environment.NewLine}{ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ShowErrorMessage(errorMsg);
            }
            else
            {
                Log.Information("Success!");
                Close();
            }
        }

        private void ShowErrorMessage(string message)
        {
            statusTextBox.AppendText(message + Environment.NewLine);
            statusTextBox.BackColor = Color.FromArgb(228, 192, 192);
        }

        private string ValidateHardwareSetup()
        {
            statusTextBox.AppendText("Reading configuration..." + Environment.NewLine);
            Log.Information("Reading configuration");
            _config = ReadConfiguration();
            var map = _config.GetSelectedMap();

            statusTextBox.AppendText("Checking hardware..." + Environment.NewLine);
            Log.Information($"Validating hardware setup '{map.Name}'");

            var sb = new StringBuilder(100);

            var errMsg = ValidateSoundCardConfiguration(map.NumChannels);
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = ValidateSyncDevice();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = ValidateLEDDevice();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = ValidateDigitimerDevices();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            return sb.ToString();
        }

        private string ValidateSyncDevice()
        {
            if (string.IsNullOrEmpty(_config.SyncComPort))
            {
                statusTextBox.AppendText("-- No sync device --" + Environment.NewLine);
                Log.Information("No sync COM port specified");
                return "";
            }

            statusTextBox.AppendText("Checking sync device..." + Environment.NewLine);
            Log.Information("Pinging sync device");

            var firmware = ConfigForm.TestComPort(_config.SyncComPort, "livin' the dream");
            if (string.IsNullOrEmpty(firmware))
            {
                string msg = $"Sync device not responding at: {_config.SyncComPort}";
                Log.Information(msg);
                return msg;
            }
            else
            {
                string msg = $"Sync device running firmware V{firmware}";
                statusTextBox.AppendText(msg + Environment.NewLine);
                Log.Information(msg);

            }
            return "";
        }

        private string ValidateLEDDevice()
        {
            if (string.IsNullOrEmpty(_config.LEDComPort))
            {
                statusTextBox.AppendText("-- No LED device --" + Environment.NewLine);
                Log.Information("No LED COM port specified");
                return "";
            }

            statusTextBox.AppendText("Checking LED device..." + Environment.NewLine);
            Log.Information("Pinging LED device");

            var firmware = ConfigForm.TestComPort(_config.SyncComPort, "lightin' the way, big man");
            if (string.IsNullOrEmpty(firmware))
            {
                string msg = $"LED device not responding at: {_config.LEDComPort}";
                Log.Information(msg);
                return msg;
            }
            else
            {
                string msg = $"LED device running firmware V{firmware}";
                statusTextBox.AppendText(msg + Environment.NewLine);
                Log.Information(msg);

            }
            return "";
        }

        private string SetScreenBrightness()
        {
            string errMsg = "";
            if (_config.ScreenBrightness >= 0)
            {
                statusTextBox.AppendText($"Setting screen brightness to {_config.ScreenBrightness}" + Environment.NewLine);
                Log.Information($"Setting screen brightness to {_config.ScreenBrightness}");
                try
                {
                    Brightness.Set(this.Handle, _config.ScreenBrightness);
                }
                catch (Exception ex)
                {
                    errMsg = "Failed to set screen brightness";
                    Log.Error($"Failed to set screen brightness: {ex.Message}");
                }
            }

            return errMsg;
        }

        private string ValidateDigitimerDevices()
        {
            var map = _config.GetSelectedMap();
            var devices = map.Items.FindAll(x => x.modality.Equals("Electric") && x.transducer.StartsWith("DS8R"));
            if (devices.Count == 0)
            {
                return "";
            }

            statusTextBox.AppendText("Initializing Digitimer devices..." + Environment.NewLine);
            string result = "";

            D128ExAPI d128 = null;
            try
            {
                d128 = new D128ExAPI();
                d128.Initialize();
                d128.GetState();
                foreach (var d in devices)
                {
                    int id = int.Parse(d.transducer.Substring(("DS8R").Length));
                    float max = float.Parse(d.extra);

                    if (d128.Devices.Contains(id))
                    {
                        d128[id].Limit = (int)(max * 10);
                        d128[id].Source = DemandSource.External;
                    }
                    else
                    {
                        var e = $"DS8R #{id} not found";
                        result += e + Environment.NewLine;
                        Log.Error(e);
                    }
                }

                var errorCode = d128.SetState();
                if (errorCode != ErrorCode.Success)
                {
                    result += "Failed to initialize Digitimer devices" + Environment.NewLine;
                    Log.Error($"Failed to set current limits ({errorCode})");
                }
            }
            catch (Exception ex)
            {
                result += "Failed to initialize Digitimer devices" + Environment.NewLine;
                Log.Error($"Failed to initialize Digitimer devices: {ex.Message}");
            }
            finally
            {
                if (d128 != null) d128.Close();
            }

            return result;
        }

        private HardwareConfiguration ReadConfiguration()
        {
            HardwareConfiguration config = null;
            if (File.Exists(FileLocations.HardwareConfigFile))
            {
                config = KFile.XmlDeserialize<HardwareConfiguration>(FileLocations.HardwareConfigFile);
            }
            if (config == null)
            {
                config = HardwareConfiguration.GetDefaultConfiguration();
            }
            return config;
        }

        private string ValidateSoundCardConfiguration(int numChannels)
        {
            statusTextBox.AppendText("Checking sound card..." + Environment.NewLine);
            Log.Information("Validating sound card");
            if (numChannels == 2) return "";

            var desiredFormat = new NAudio.Wave.WaveFormatExtensible(48000, 16, 8, (int)ChannelMapping.Surround7point1);

            var device = Utilities.GetDefaultDevice();
            Log.Information($"Default device: {device.FriendlyName}");

            // is default device already in Surround 7.1 format?
            var audioClient = device.AudioClient;
            var currentFormat = audioClient.MixFormat;

            if (currentFormat.ChannelMask == (int)ChannelMapping.Surround7point1)
            {
                Log.Information("Default device is in 7.1 mode");
                return "";
            }

            Log.Information($"Default device format has {currentFormat.Channels} channel(s)");

            // No: can it be put in that format?
            var supports71 = audioClient.IsFormatSupported(AudioClientShareMode.Shared, desiredFormat);
            audioClient.Dispose();
            device.Properties.SetValue(PKEY.PKEY_AudioEndpoint_PhysicalSpeakers, PropVariant.FromUInt((uint)ChannelMapping.Surround7point1));

            // if audio enhancements are enabled, the device driver may respond capable of 7.1 on the basis that
            // it has an APO enabling it to convert 7.1 to stereo. To which we say, oh yeah, how many jacks ya got?
            // one jack? that's cute sweetheart. Fuck all the way off.
            uint numJacks = GetJackCount(device);
            supports71 &= numJacks > 1;

            if (supports71)
            {
                Log.Information("Setting device format to 7.1");
                Utilities.SetDeviceFormat(device, desiredFormat);
                return "";
            }

            // Still no: is there another device that supports 7.1?
            var deviceEnumerator = new MMDeviceEnumerator();
            foreach (var d in deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eRender, DEVICE_STATE.DEVICE_STATE_ACTIVE))
            {
                audioClient = d.AudioClient;

                var supports = audioClient.IsFormatSupported(AudioClientShareMode.Shared, desiredFormat);
                audioClient.Dispose();

                supports &= GetJackCount(d)>1;

                if (supports)
                {
                    Log.Information($"Changing default device to {d.FriendlyName}");
                    d.Selected = true;
                    Log.Information("Setting device format to 7.1");
                    Utilities.SetDeviceFormat(d, desiredFormat);

                    var errMsg = RestartService();
                    return errMsg;
                }
                Log.Information($"{d.FriendlyName} does not support 7.1");
            }

            return "No 7.1 audio card was found";
        }

        private uint GetJackCount(MMDevice device)
        {
            var jd = device.DeviceTopology.GetConnector(0).GetConnectedTo.GetPart.JackDescription;
            uint numJacks = (jd == null) ? 0 : jd.Count;
            return numJacks;
        }

        private string RestartService()
        {
            string errMsg = "";
            try
            {
                var processStartInfo = new ProcessStartInfo("net.exe", "stop audiosrv /y");
                processStartInfo.UseShellExecute = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.WorkingDirectory = Environment.SystemDirectory;

                Log.Information("stopping audiosrv");
                var process = Process.Start(processStartInfo);
                process.WaitForExit(10000);

                if (!process.HasExited)
                {
                    process.Kill();
                    Log.Error("Timed out stopping audio service");
                    errMsg = "Error restarting audio service";
                }

                Log.Information("starting audiosrv");
                processStartInfo.Arguments = "start audiosrv";
                process = Process.Start(processStartInfo);
                process.WaitForExit(10000);
                if (!process.HasExited)
                {
                    process.Kill();
                    Log.Error("Timed out starting audio service");
                    return "Error restarting audio service";
                }

            }
            catch (Exception ex)
            {
                Log.Error("error restarting audiosrv:"  + ex.Message);
                errMsg = "Error restarting audio service";
            }

            return errMsg;
        }
    }
}
