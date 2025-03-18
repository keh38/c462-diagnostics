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

using KLib;

namespace Launcher
{
    public partial class MainForm : Form
    {
        bool _configButtonPressed = false;
        Timer _timer;
        int _delayTime = 5000;

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
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await StartLogging();
            Log.Information($"Started {GetVersion()}");
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && !_configButtonPressed)
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
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EPL", "Logs", "SandboxLauncher-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    flushToDiskInterval: TimeSpan.FromSeconds(30),
                    buffered: true)
                .CreateLogger();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            if (!_configButtonPressed)
            {
                LaunchUnityApp();
            }
        }

        private void LaunchUnityApp()
        {
            var errorMsg = ValidateHardwareSetup();

            if (!string.IsNullOrEmpty(errorMsg))
            {
                statusTextBox.AppendText(errorMsg + Environment.NewLine);
                statusTextBox.BackColor = Color.FromArgb(228, 192, 192);
            }
            else
            {
                Close();
            }
        }

        private string ValidateHardwareSetup()
        {
            statusTextBox.AppendText("Reading configuration..." + Environment.NewLine);
            Log.Information("Reading configuration");
            var config = ReadConfiguration();
            var map = config.GetSelectedMap();

            statusTextBox.AppendText("Checking hardware..." + Environment.NewLine);
            Log.Information($"Validating hardware setup '{map.Name}'");

            var errMsg = ValidateSoundCardConfiguration(map.NumChannels);

            return errMsg;
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
            uint numJacks = device.DeviceTopology.GetConnector(0).GetConnectedTo.GetPart.JackDescription.Count;
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

        private string RestartService()
        {
            string errMsg = "";
            try
            {
                var processStartInfo = new ProcessStartInfo("net.exe", "stop audiosrv");
                processStartInfo.UseShellExecute = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.WorkingDirectory = Environment.SystemDirectory;

                Log.Information("stopping audiosrv");
                var start = Process.Start(processStartInfo);
                start.WaitForExit();

                Log.Information("starting audiosrv");
                processStartInfo.Arguments = "start audiosrv";
                start = Process.Start(processStartInfo);
                start.WaitForExit();
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
