using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using C462.Shared;

public class DigitimerTrigger
{
    private SerialPort _serialPort;

    public bool Connected { get { return _serialPort != null; } }

    public DigitimerTrigger() { }

    public bool Initialize(string comPort)
    {
        if (string.IsNullOrEmpty(comPort))
        {
            _serialPort = null;
            return true;
        }

        _serialPort = new SerialPort();
        _serialPort.PortName = comPort;
        _serialPort.BaudRate = 115200;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Handshake = Handshake.None;
        _serialPort.NewLine = "\n";

        // Need the next two lines: https://forum.arduino.cc/t/c-program-can-t-receive-any-data-from-arduino-serialusb-class/956418/7
        _serialPort.RtsEnable = true;
        _serialPort.DtrEnable = true;

        _serialPort.ReadTimeout = 1000;
        _serialPort.WriteTimeout = 1000;

        bool success = false;
        try
        {
            _serialPort.Open();
            _serialPort.Write("'sup\n");
            string response = _serialPort.ReadLine();
            Debug.Log($"[Digitimer trigger] response to greeting: '{response}'");

            _serialPort.Close();

            success = true;
        }
        catch (Exception ex)
        {
            Debug.Log($"[DigitimerTrigger::InitializeSerialPort] {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }

        return success;
    }

    public bool EnableTrigger()
    {
        if (_serialPort == null)
        {
            return true;
        }

        var success = false;

        try
        {
            _serialPort.Open();
            _serialPort.Write($"enable\n");
            string response = _serialPort.ReadLine();
            success = (response.StartsWith("OK"));
        }
        catch (Exception ex)
        {
            Debug.Log($"[Digitimer trigger] error enabling: {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }

        return success;
    }

    public bool DisableTrigger()
    {
        if (_serialPort == null)
        {
            return true;
        }

        var success = false;

        try
        {
            _serialPort.Open();
            _serialPort.Write($"disable\n");
            string response = _serialPort.ReadLine();
            success = (response.StartsWith("OK"));
        }
        catch (Exception ex)
        {
            Debug.Log($"[Digitimer trigger] error disabling: {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }

        return success;
    }
}
