using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class LEDTestForm : Form
    {
        bool _ignoreEvents = false;
        string _comPort;
        int _baudRate;
        float _gamma;

        SerialPort _serialPort;

        int _numPixels;

        int _red;
        int _green;
        int _blue;
        int _white;

        public LEDTestForm(string comPort, int baudRate, int numPixels, float gamma)
        {
            InitializeComponent();

            _comPort = comPort;
            _baudRate = baudRate;
            _numPixels = numPixels;
            _gamma = gamma;
        }

        private void LEDTestForm_Shown(object sender, EventArgs e)
        {
            var success = OpenSerialPort();
            if (!success)
            {
                statusTextBox.Text = "Failed to initialize serial port";
                statusTextBox.Visible = true;

                redSlider.Enabled = false;
                greenSlider.Enabled = false;
                blueSlider.Enabled = false;
                whiteSlider.Enabled = false;

                return;
            }

            SetNumPixels(_numPixels);
            GetColor();
        }
        private void LEDTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("closing serial port");
            CloseSerialPort();
        }

        private bool OpenSerialPort()
        {
            bool success = false;

            _serialPort = new SerialPort();
            _serialPort.PortName = _comPort;
            _serialPort.BaudRate = _baudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.NewLine = "\n";

            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;

            try
            {
                _serialPort.Open();
                _serialPort.Write("'sup\n");
                string response = _serialPort.ReadLine();
                if (response.StartsWith("lightin' the way, big man"))
                {
                    success = true;
                }
            }
            catch (Exception) { }

            if (!success)
            {
                _serialPort.Close();
                _serialPort = null;
            }

            return success;
        }

        private void CloseSerialPort()
        {
            try
            {
                _serialPort.Close();
            }
            catch { }
        }

        private void SetNumPixels(int numPixels)
        {
            if (_serialPort == null) return;

            bool success = false;

            try
            {
                _serialPort.Write($"setnumpixels {numPixels}\n");
                string response = _serialPort.ReadLine();
                success = response.Equals("OK");
            }
            catch { }

            if (!success)
            {
                statusTextBox.Text = "Error setting color";
                statusTextBox.Visible = true;
            }
        }
        private void GetColor()
        {
            if (_serialPort == null) return;

            bool success = false;

            try
            {
                _serialPort.Write($"getcolor\n");
                string response = _serialPort.ReadLine();
                success = response.StartsWith("OK");
                if (success)
                {
                    var parts = response.Split(' ');
                    if (parts.Length == 5)
                    {
                        _red = (int)(100 * (float)int.Parse(parts[1]) / 255f);
                        _green = (int)(100 * (float)int.Parse(parts[2]) / 255f);
                        _blue = (int)(100 * (float)int.Parse(parts[3]) / 255f);
                        _white = (int)(100 * (float)int.Parse(parts[4]) / 255f);

                        ShowColor();
                    }
                }
            }
            catch { }

            if (!success)
            {
                statusTextBox.Text = "Error setting color";
                statusTextBox.Visible = true;
            }
        }

        private void SetColor()
        {
            if (_serialPort == null) return;

            bool success = false;

            try
            {
                _serialPort.Write($"setcolor {ApplyGamma(_red)} {ApplyGamma(_green)} {ApplyGamma(_blue)} {ApplyGamma(_white)}\n");
                string response = _serialPort.ReadLine();
                success = response.Equals("OK");
            }
            catch { }

            if (!success)
            {
                statusTextBox.Text = "Error setting color";
                statusTextBox.Visible = true;
            }
        }

        private int ApplyGamma(int intensity)
        {
            return (int) Math.Round(Math.Pow((float)intensity/100, _gamma) * 255);
        }

        private void ShowColor()
        {
            _ignoreEvents = true;

            redSlider.Value = _red;
            greenSlider.Value = _green;
            blueSlider.Value = _blue;  
            whiteSlider.Value = _white;

            _ignoreEvents = false;
        }

        private void redSlider_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _red = redSlider.Value;
                SetColor();
            }
        }

        private void greenSlider_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _green = greenSlider.Value;
                SetColor();
            }
        }

        private void blueSlider_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _blue = blueSlider.Value;
                SetColor();
            }
        }

        private void whiteSlider_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _white = whiteSlider.Value;
                SetColor();
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            _red = 0;
            _green = 0;
            _blue = 0;
            _white = 0;

            ShowColor();
            SetColor();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
