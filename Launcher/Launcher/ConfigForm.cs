using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using D128NET;
using KLib;

using Serilog;

namespace Launcher
{
    [SupportedOSPlatform("windows")]
    public partial class ConfigForm : Form
    {
        private HardwareConfiguration _config;
        private List<string> _digitimerDevices;

        private AdapterMap _map;

        private bool _ignoreEvents;

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            _config = null;
            if (File.Exists(FileLocations.HardwareConfigFile))
            {
                _config = KFile.XmlDeserialize<HardwareConfiguration>(FileLocations.HardwareConfigFile);
            }
            if (_config == null)
            {
                _config = HardwareConfiguration.GetDefaultConfiguration();
            }

            _digitimerDevices = EnumerateDigitimerDevices();

            mapDropDown.Items.Clear();
            mapDropDown.Items.AddRange([.. _config.AdapterMaps.Select(x => x.Name)]);
            mapDropDown.SelectedIndex = _config.AdapterMaps.FindIndex(x => x.Name.Equals(_config.CurrentAdapterMap));

            FillComPortDropDown();
            FillLEDComPortDropDown();

            _ignoreEvents = true;
            gammaNumeric.FloatValue = _config.LEDGamma;
            brightnessNumeric.IntValue = _config.ScreenBrightness;
            _ignoreEvents = false;
        }

        private void FillTable(AdapterMap map)
        {
            if (map == null) return;
            _map = map;

            _ignoreEvents = true;

            dataGridView.Rows.Clear();
            foreach (var i in map.Items)
            {
                AddRow(i);
            }

            _ignoreEvents = false;
        }

        private void FillComPortDropDown()
        {
            comPortDropDown.Items.Clear();
            var ports = SerialPort.GetPortNames();
            comPortDropDown.Items.AddRange(ports);
            comPortDropDown.Items.Add("none");

            _ignoreEvents = true;
            comPortDropDown.SelectedIndex = ports.ToList().IndexOf(_config.SyncComPort);
            _ignoreEvents = false;
        }

        private void FillLEDComPortDropDown()
        {
            ledComPortDropDown.Items.Clear();
            var ports = SerialPort.GetPortNames();
            ledComPortDropDown.Items.AddRange(ports);
            ledComPortDropDown.Items.Add("none");

            _ignoreEvents = true;
            ledComPortDropDown.SelectedIndex = ports.ToList().IndexOf(_config.LEDComPort);
            //testButton.Enabled = ledComPortDropDown.SelectedIndex > -1;
            _ignoreEvents = false;
        }

        private void AddRow(AdapterMap.AdapterSpec spec)
        {
            int rowIndex = dataGridView.Rows.Add();
            var cells = dataGridView.Rows[rowIndex].Cells;

            cells["Jack"].Value = spec.jackName;
            cells["Modality"].Value = spec.modality;
            cells["Transducer"].Value = spec.transducer;
            cells["Channel"].Value = spec.location;
            cells["Extra"].Value = spec.extra;

            if (spec.modality == "Audio")
            {
                dataGridView.Rows[rowIndex].ReadOnly = true;
            }
            if (spec.modality == "Electric")
            {
                cells["Transducer"].Style.ForeColor = (_digitimerDevices.Contains(spec.transducer)) ? Color.Black : Color.Red;
            }
            else
            {
                cells["Extra"].ReadOnly = true;
                cells["Extra"].Style.BackColor = Color.LightGray;
            }
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mapDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillTable(_config.AdapterMaps[mapDropDown.SelectedIndex]);
            _config.CurrentAdapterMap = mapDropDown.Text;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            KFile.XmlSerialize(_config, FileLocations.HardwareConfigFile);
        }

        private void comPortDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _config.SyncComPort = comPortDropDown.Text.Equals("none") ? "" : comPortDropDown.Text;
            }
        }

        private async void detectButton_Click(object sender, EventArgs e)
        {
            messageLabel.Visible = false;
            detectButton.Enabled = false;

            _config.SyncComPort = "";

            var firmware = "";

            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                firmware = await Task.Run(() => TestComPort(port, 115200, "livin' the dream"));
                if (!string.IsNullOrEmpty(firmware))
                {
                    _config.SyncComPort = port;
                    break;
                }
            }

            FillComPortDropDown();

            if (string.IsNullOrEmpty(_config.SyncComPort))
            {
                messageLabel.Text = "No sync device found.";
                messageLabel.Visible = true;
            }
            else
            {
                messageLabel.Text = $"Sync firmware V{firmware}";
                messageLabel.Visible = true;
            }

            detectButton.Enabled = true;
        }

        public static string TestComPort(string comPort, int baudRate, string targetResponse)
        {
            string firmwareVersion = "";

            var serialPort = new SerialPort();
            serialPort.PortName = comPort;
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.NewLine = "\n";

            // Need the next two lines: https://forum.arduino.cc/t/c-program-can-t-receive-any-data-from-arduino-serialusb-class/956418/7
            if (baudRate > 9600)
            {
                serialPort.RtsEnable = true;
                serialPort.DtrEnable = true;
            }

            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;

            try
            {
                serialPort.Open();
                serialPort.Write("'sup\n");
                string response = serialPort.ReadLine();
                if (response.StartsWith(targetResponse))
                {
                    var parts = response.Split(':');
                    if (parts.Length > 1)
                    {
                        firmwareVersion = parts[1];
                    }
                    else
                    {
                        firmwareVersion = "???";
                    }
                }
                serialPort.Close();
            }
            catch (Exception) { }
            finally
            {
                serialPort.Close();
            }
            return firmwareVersion;
        }

        private List<string> EnumerateDigitimerDevices()
        {
            var devices = new List<string>();

            //devices.Add("DSR372");

            D128ExAPI d128 = null;
            try
            {
                d128 = new D128ExAPI();
                d128.Initialize();
                d128.GetState();
                foreach (var d in d128.Devices)
                {
                    devices.Add("DS8R" + d);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = "Error enumerating Digitimer devices";
                messageLabel.Visible = true;
                Log.Error("Error enumerating Digitimer devices: " + ex.Message);
            }
            finally
            {
                if (d128 != null) d128.Close();
            }

            return devices;
        }

        private void dsrDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                dsrDropDown.Visible = false;

                int rowIndex = dataGridView.CurrentCell.RowIndex;
                _map.Items[rowIndex].transducer = dsrDropDown.Text;
                dataGridView.CurrentCell.Value = dsrDropDown.Text;
                dataGridView.CurrentCell.Style.ForeColor = (_digitimerDevices.Contains(dsrDropDown.Text)) ? Color.Black : Color.Red;
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_ignoreEvents || _map == null) return;

            int rowIndex = dataGridView.CurrentCell.RowIndex;
            var cells = dataGridView.Rows[rowIndex].Cells;

            if (dataGridView.CurrentCell.OwningColumn.Name == "Modality")
            {
                _map.Items[rowIndex].modality = dataGridView.CurrentCell.Value.ToString();
            }
            else if (dataGridView.CurrentCell.OwningColumn.Name == "Transducer")
            {
                _map.Items[rowIndex].transducer = dataGridView.CurrentCell.Value.ToString();
            }
            else if (dataGridView.CurrentCell.OwningColumn.Name == "Channel")
            {
                _map.Items[rowIndex].transducer = dataGridView.CurrentCell.Value.ToString();
            }
            else if (dataGridView.CurrentCell.OwningColumn.Name == "Extra")
            {
                if (float.TryParse(dataGridView.CurrentCell.Value as string, out float max))
                {
                    _map.Items[rowIndex].extra = max.ToString();
                }
                else
                {
                    dataGridView.CurrentCell.Value = _map.Items[rowIndex].extra;
                }
            }

        }

        private void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents && dataGridView.CurrentCell.ColumnIndex == 1 && dataGridView.IsCurrentCellDirty)
            {
                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_map.Items[e.RowIndex].modality != "Electric" || e.ColumnIndex != 2)
            {
                dsrDropDown.Visible = false;
                return;
            }

            Rectangle r = dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

            dsrDropDown.Items.Clear();
            dsrDropDown.Items.AddRange([.. _digitimerDevices]);
            dsrDropDown.Items.Add("---");
            dsrDropDown.Tag = false;
            dsrDropDown.ForeColor = Color.Black;

            int index = _digitimerDevices.IndexOf(dataGridView.CurrentCell.Value as string);
            if (index < 0)
            {
                dsrDropDown.Items.Insert(0, dataGridView.CurrentCell.Value as string);
                dsrDropDown.Tag = true;
                dsrDropDown.ForeColor = Color.Red;
                index = 0;
            }


            _ignoreEvents = true;
            dsrDropDown.SelectedIndex = index;
            _ignoreEvents = false;

            dsrDropDown.Top = r.Top + dataGridView.Location.Y;
            dsrDropDown.Left = dataGridView.Location.X + r.Right - dsrDropDown.Width;
            dsrDropDown.Visible = true;
        }

        private void dsrDropDown_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            // a dropdownlist may initially have no item selected, so skip the highlighting:
            if (e.Index >= 0)
            {
                Graphics g = e.Graphics;
                Brush brush = new SolidBrush(e.BackColor);
                Brush tBrush = ((bool)dsrDropDown.Tag && e.Index == 0) ? new SolidBrush(Color.Red) : new SolidBrush(Color.Black);

                g.FillRectangle(brush, e.Bounds);
                e.Graphics.DrawString(dsrDropDown.Items[e.Index].ToString(), e.Font,
                           tBrush, e.Bounds, StringFormat.GenericDefault);
                brush.Dispose();
                tBrush.Dispose();
            }
            e.DrawFocusRectangle();
        }

        private void dataGridView_Leave(object sender, EventArgs e)
        {
            //if (!dsrDropDown.Focused) dsrDropDown.Visible = false;
        }

        private async void ledDetectButton_Click(object sender, EventArgs e)
        {
            messageLabel.Visible = false;
            ledDetectButton.Enabled = false;

            _config.LEDComPort = "";

            string firmware = "";

            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                firmware = await Task.Run(() => TestComPort(port, 9600, "lightin' the way, big man"));
                if (!string.IsNullOrEmpty(firmware))
                {
                    _config.LEDComPort = port;
                    break;
                }
            }

            FillLEDComPortDropDown();

            if (string.IsNullOrEmpty(_config.LEDComPort))
            {
                messageLabel.Text = "No LED device found.";
                messageLabel.Visible = true;
            }
            else
            {
                messageLabel.Text = $"LED firmware V{firmware}";
                messageLabel.Visible = true;
            }

            ledDetectButton.Enabled = true;
        }

        private void brightnessNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _config.ScreenBrightness = brightnessNumeric.IntValue;
            }
            if (_config.ScreenBrightness >= 0)
            {
                KLib.Brightness.Set(this.Handle, _config.ScreenBrightness);
            }
        }

        private void ledComPortDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _config.LEDComPort = ledComPortDropDown.Text.Equals("none") ? "" : ledComPortDropDown.Text;
                testButton.Enabled = !ledComPortDropDown.Text.Equals("none");
            }
        }

        private void gammaNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _config.LEDGamma = gammaNumeric.FloatValue;
            }
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            var firmware = await Task.Run(() => TestComPort(_config.LEDComPort, 9600, "lightin' the way, big man"));
            if (!string.IsNullOrEmpty(firmware))
            {
                messageLabel.Text = "No LED device found.";
                messageLabel.Visible = true;
                return;
            }

            var dlg = new LEDTestForm(_config.LEDComPort, 9600, _config.LEDGamma);
            dlg.ShowDialog();
        }
    }
}
