using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;

public class LEDController : MonoBehaviour
{
    private bool _isPortOpen = false;
    private float _gamma = 2.8f;

    private SerialPort _serialPort;

    public bool IsInitialized { get; private set; }

    public bool Initialize(string comPort, int numPixels, float gamma)
    {
        bool success = false;

        _gamma = gamma;

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

            SetNumPixels(numPixels);
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
        catch (Exception ex)
        {
            Debug.Log($"[LEDController] error setting number of pixels: {ex.Message}");
        }
    }

    public bool SetColorFromString(string colorSpec)
    {
        var parts = colorSpec.Split(',');
        int r = ApplyGamma(float.Parse(parts[0]));
        int g = ApplyGamma(float.Parse(parts[1]));
        int b = ApplyGamma(float.Parse(parts[2]));
        int w = ApplyGamma(float.Parse(parts[3]));

        HTS_Server.SendMessage("ChangedLEDColor", colorSpec);

        return SetColor(r, g, b, w);
    }

    private int ApplyGamma(float intensity)
    {
        return Mathf.RoundToInt(Mathf.Pow(intensity, _gamma) * 255);
    }

    private float ByteValueToFloat(int value, float gamma)
    {
        return Mathf.Pow((float)value / 255f, 1 / gamma);
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
    public string GetColor()
    {
        string color = "none";

        try
        {
            _serialPort.Open();
            _serialPort.Write($"getcolor\n");
            string response = _serialPort.ReadLine();
            var success = response.StartsWith("OK");
            if (success)
            {
                var parts = response.Split(' ');
                if (parts.Length == 5)
                {
                    var r = ByteValueToFloat(int.Parse(parts[1]), _gamma);
                    var g = ByteValueToFloat(int.Parse(parts[2]), _gamma);
                    var b = ByteValueToFloat(int.Parse(parts[3]), _gamma);
                    var w = ByteValueToFloat(int.Parse(parts[4]), _gamma);

                    Debug.Log($"get color: {response} -> {r},{g},{b},{w}");
                    color = $"{r},{g},{b},{w}";
                }
            }
        }
        catch { }
        finally { _serialPort.Close(); }

        return color;
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

    public bool SetColorDynamically(float intensity)
    {
        return SetColorDynamically(ApplyGamma(intensity));
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
            string response = _serialPort.ReadLine();
            bool success = response.Equals("OK");
            if (!success)
            {
                Debug.Log("[LEDController] negative response clearing display.");
            }
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
