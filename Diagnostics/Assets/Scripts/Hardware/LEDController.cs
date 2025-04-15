using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class LEDController : MonoBehaviour
{
    private bool _isPortOpen = false;

    private SerialPort _serialPort;

    public bool IsInitialized { get; private set; }

    public bool InitializeSerialPort(string comPort)
    {
        bool success = false;

        _serialPort = new SerialPort();
        _serialPort.PortName = comPort;
        _serialPort.BaudRate = 9600;
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
            Debug.Log($"[LEDController] response to greeting: '{response}'");
            _serialPort.Close();

            IsInitialized = true;
            success = true;
        }
        catch (Exception ex)
        {
            Debug.Log($"[LEDController::InitializeSerialPort] {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }

        return success;
    }

    public bool SetColor(int red, int green, int blue, int white)
    {
        var success = false;

        try
        {
            _serialPort.Open();
            _serialPort.Write($"setcolor {red} {green} {blue} {white}\n");
            string response = _serialPort.ReadLine();
            success = response.Equals("OK");
        }
        catch (Exception ex)
        {
            Debug.Log($"[LEDController] error setting color: {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }

        return success;
    }

    public bool Open()
    {
        bool success = false;
        try
        {
            _serialPort.Open();
            success = true;
            _isPortOpen = true;
        }
        catch (Exception ex)
        {
            Debug.Log($"[LEDController] error opening serial port: {ex.Message}");
        }
        return success;
    }

    public bool SetColorDynamically(int intensity)
    {
        //return SetColorDynamically(intensity, intensity, intensity, intensity);
        return SetColorDynamically(0, 0, 0, intensity);
    }

    public bool SetColorDynamically(int red, int green, int blue, int white)
    {
        var success = false;

        if (!_isPortOpen) return false;

        try
        {
            _serialPort.Write($"setcolor {red} {green} {blue} {white}\n");
            string response = _serialPort.ReadLine();
            success = response.Equals("OK");
        }
        catch (Exception ex)
        {
            Debug.Log($"[LEDController] error setting color dynamically: {ex.Message}");
            _isPortOpen = false;
            _serialPort.Close();
        }

        return success;
    }

    public void Close()
    {
        try
        {
            _serialPort.Close();
            Clear();
        }
        catch (Exception ex) { }

        _isPortOpen = false;
    }

    public void Clear()
    {
        if (!IsInitialized) return;

        try
        {
            _serialPort.Open();
            _serialPort.Write("clear\n");
        }
        catch (Exception ex)
        {
            Debug.Log($"[LEDController] error clearing display: {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }
    }

    public void CleanUp()
    {
        Clear();
    }

}
