using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KLib;

namespace Launcher
{
    public partial class ConfigForm : Form
    {
        private HardwareConfiguration _config;
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

            mapDropDown.Items.Clear();
            mapDropDown.Items.AddRange(_config.AdapterMaps.Select(x => x.Name).ToArray());
            mapDropDown.SelectedIndex = _config.AdapterMaps.FindIndex(x => x.Name.Equals(_config.CurrentAdapterMap));

            comPortDropDown.Items.Clear();
            var ports = SerialPort.GetPortNames();
            comPortDropDown.Items.AddRange(ports);

            _ignoreEvents = true;
            comPortDropDown.SelectedIndex = ports.ToList().IndexOf(_config.SyncComPort);
            _ignoreEvents = false;

        }

        private void FillTable(AdapterMap map)
        {
            if (map == null) return;

            _ignoreEvents = true;

            dataGridView.Rows.Clear();
            foreach (var i in map.Items)
            {
                AddRow(i);
            }

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
            if (spec.modality != "Electric")
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
                _config.SyncComPort = comPortDropDown.Text;
             }
        }
    }
}
