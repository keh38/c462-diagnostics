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

using D128NET;
using C462.Shared;
using C462.Shared.Arduino;
using C462.Shared.UI;
using KLib;
using KLib.IO;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Launcher
{
    [SupportedOSPlatform("windows")]
    public partial class MainForm : Form
    {
        bool _buttonPressAllowed = false;
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
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            var alreadyRunning = CheckForExistingProcess();
            if (alreadyRunning)
            {
                _timer = new Timer();
                _timer.Interval = 2000;
                _timer.Tick += Timer_Tick_Quit;
                _timer.Enabled = true;
                return;
            }

            _buttonPressAllowed = true;
            statusTextBox.Text = "Starting..." + Environment.NewLine;

            await StartLogging();
            Log.Information($"Started {GetVersion()}");

            var commandLineArgs = Environment.GetCommandLineArgs().ToList();
            if (commandLineArgs.Contains("-nodelay"))
            {
                LaunchUnityApp();
                return;
            }
            
            _timer = new Timer();
            _timer.Interval = _delayTime;
            _timer.Tick += Timer_Tick;
            _timer.Enabled = true;
        }

        private bool CheckForExistingProcess()
        {
            if (Process.GetProcessesByName("Hearing Test Suite").Length > 0)
            {
                statusTextBox.Text = "Already running." + Environment.NewLine;
                return true;
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_buttonPressAllowed) return true;

            if (keyData == Keys.Enter && !_launchStarted)
            {
                LaunchUnityApp();
                return true;
            }

            if ((keyData == Keys.F2 || keyData == Keys.C) && !_configButtonPressed)
            {
                _timer.Enabled = false;
                _configButtonPressed = true;
                Log.Information("Config button pressed");
                ShowConfigDialog();
                _configButtonPressed = false;
                LaunchUnityApp();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                Close();
            }
            return true;
        }

        private void ShowConfigDialog()
        {
            var dlg = new HardwareConfigForm();
            dlg.Icon = this.Icon;
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
                    Path.Combine(SharedFileLocations.SharedFolder, "Logs", "HTSLauncher-.txt"),
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

        private void Timer_Tick_Quit(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            Close();
        }

        private async void LaunchUnityApp()
        {
            _buttonPressAllowed = false;
            _launchStarted = true;

            string errorMsg = "";
            try
            {
                errorMsg = await ValidateHardwareSetup();
            }
            catch (Exception ex)
            {
                errorMsg = "Failed to initialize hardware";
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ShowErrorMessage(errorMsg);
                _buttonPressAllowed = true;
                return;
            }

            statusTextBox.AppendText("Starting \"Hearing Test Suite\"..." + Environment.NewLine);
            Log.Information("Starting SOS");

                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
                var index = folder.IndexOf("Launcher");
                folder = Path.Combine(folder.Substring(0, index - 1), "Diagnostics", "Build");
                return;
#else
                // up one more level
                folder = Path.GetDirectoryName(folder);
#endif
                var htsPath = Path.Combine(folder, "Hearing Test Suite.exe");

                try
                {
                    Log.Information($"App path = {htsPath}");
                    string args = "-screen-fullscreen 1";
                    
                    if (_config.RunWindowed)
                    {
                        args = $"-screen-fullscreen 0 -screen-width {_config.ScreenWidth} -screen-height {_config.ScreenHeight}";
                    }
                    else
                    {
                        string key = @"SOFTWARE\Eaton-Peabody Labs\Hearing Test Suite";
                        using (var view64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                        {
                            using (var subKey = view64.OpenSubKey(key, true))
                            {
                                subKey.SetValue("Screenmanager Resolution Use Native_h1405027254", 1, RegistryValueKind.DWord);
                            }
                        }
                    }

                    var process = Process.Start(htsPath, args);
                    await WaitForWindowAsync(process, timeoutMs: 30_000)
                        .ContinueWith(_ => Close(), TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
                {
                    errorMsg = "Error starting app";
                    Log.Error($"Error starting app:{Environment.NewLine}{ex.Message}");
                }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ShowErrorMessage(errorMsg);
                _buttonPressAllowed = true;
                return;
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

        private void ShowErrorMessage(string message)
        {
            statusTextBox.AppendText(message);
            statusTextBox.BackColor = Color.FromArgb(228, 192, 192);
        }

        private async Task<string> ValidateHardwareSetup()
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

            errMsg = await ValidateSyncDevice();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = await ValidateLEDDevice();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = await ValidateDigitimerDevice();
            if (!string.IsNullOrEmpty(errMsg)) sb.AppendLine(errMsg);

            errMsg = ValidateDigitimerDevices();
            if (!string.IsNullOrEmpty(errMsg)) sb.Append(errMsg);

            return sb.ToString();
        }

        private async Task<string> ValidateSyncDevice()
        {
            if (string.IsNullOrEmpty(_config.SyncComPort))
            {
                statusTextBox.AppendText("-- No sync device --" + Environment.NewLine);
                Log.Information("No sync COM port specified");
                return "";
            }

            statusTextBox.AppendText("Checking sync device..." + Environment.NewLine);
            Log.Information("Pinging sync device");

            var firmware = await ArduinoDiscoveryService.TestComPortForDeviceType(_config.SyncComPort, ArduinoDeviceType.AudioSync);
            if (firmware == null)
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

        private async Task<string> ValidateLEDDevice()
        {
            if (string.IsNullOrEmpty(_config.LEDComPort))
            {
                statusTextBox.AppendText("-- No LED device --" + Environment.NewLine);
                Log.Information("No LED COM port specified");
                return "";
            }

            statusTextBox.AppendText("Checking LED device..." + Environment.NewLine);
            Log.Information("Pinging LED device");

            var firmware = await ArduinoDiscoveryService.TestComPortForDeviceType(_config.LEDComPort, ArduinoDeviceType.LEDController);
            if (firmware == null)
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

        private async Task<string> ValidateDigitimerDevice()
        {
            if (string.IsNullOrEmpty(_config.DigitimerComPort))
            {
                statusTextBox.AppendText("-- No Digitimer trigger device --" + Environment.NewLine);
                Log.Information("No Digitimer trigger COM port specified");
                return "";
            }

            statusTextBox.AppendText("Checking Digitimer trigger device..." + Environment.NewLine);
            Log.Information("Pinging Digitimer trigger device");

            var firmware = await ArduinoDiscoveryService.TestComPortForDeviceType(_config.LEDComPort, ArduinoDeviceType.DigitimerTrigger);
            if (firmware == null)
            {
                string msg = $"Digitimer trigger device not responding at: {_config.LEDComPort}";
                Log.Information(msg);
                return msg;
            }
            else
            {
                string msg = $"Digitimer trigger device running firmware V{firmware}";
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
                        d128[id].Source = D128NET.DemandSource.External;
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
            if (File.Exists(SharedFileLocations.HardwareConfigFile))
            {
                config = Files.XmlDeserialize<HardwareConfiguration>(SharedFileLocations.HardwareConfigFile);
            }
            if (config == null)
            {
                config = HardwareConfiguration.GetDefaultConfiguration();
            }
            return config;
        }

        private string ValidateSoundCardConfiguration(int numChannels)
        {
            var magicKey = new PROPERTYKEY(Guid.Parse("3d6e1656-2e50-4c4c-8d85-d0acae3c6c68"), 2);

            statusTextBox.AppendText("Checking sound card..." + Environment.NewLine);
            Log.Information("Validating sound card");
            if (numChannels == 2) return "";

            var desiredFormat = new NAudio.Wave.WaveFormatExtensible(48000, 16, 8, (int)ChannelMapping.Surround7point1);

            var device = CoreAudio.Utilities.GetDefaultDevice();
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
                CoreAudio.Utilities.SetDeviceFormat(device, desiredFormat);
                //Utilities.SetDeviceFormat(device, magicKey, desiredFormat);

                var errMsg = RestartService();
                return errMsg;
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
                    CoreAudio.Utilities.SetDeviceFormat(d, desiredFormat);

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
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.WorkingDirectory = Environment.SystemDirectory;

                Log.Information("stopping audiosrv");
                var process = Process.Start(processStartInfo);

                var stdError = process.StandardError.ReadToEnd();
                process.WaitForExit(10000);

                if (!process.HasExited || !string.IsNullOrEmpty(stdError))
                {
                    process.Kill();
                    if (!string.IsNullOrWhiteSpace(stdError))
                    {
                        Log.Error($"Error stopping audio service:{Environment.NewLine}{stdError}");
                    }
                    else
                    {
                        Log.Error("Timed out stopping audio service");
                    }
                    errMsg = "Error restarting audio service";
                }

                Log.Information("starting audiosrv");
                processStartInfo.Arguments = "start audiosrv";
                process = Process.Start(processStartInfo);

                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit(10000);

                if (!process.HasExited || !string.IsNullOrEmpty(stdError))
                {
                    process.Kill();
                    if (!string.IsNullOrWhiteSpace(stdError))
                    {
                        Log.Error($"Error starting audio service:{Environment.NewLine}{stdError}");
                    }
                    else
                    {
                        Log.Error("Timed out starting audio service");
                    }
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
