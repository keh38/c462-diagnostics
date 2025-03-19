using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SyncPulseDetector : MonoBehaviour
{
    public class SyncPulseEvent
    {
        public enum Result { NotDetected, Detected, Error}

        public long systemTime = 0;
        public double offset = double.NaN;
        public double rtt = double.NaN;
        public Result result = Result.NotDetected;
    }

    private SerialPort _serialPort;

    public void InitializeSerialPort(string comPort)
    {
        _serialPort = new SerialPort();
        _serialPort.PortName = comPort;
        _serialPort.BaudRate = 115200;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Handshake = Handshake.None;
        _serialPort.NewLine = "\r";

        // Need the next two lines: https://forum.arduino.cc/t/c-program-can-t-receive-any-data-from-arduino-serialusb-class/956418/7
        _serialPort.RtsEnable = true;
        _serialPort.DtrEnable = true;

        _serialPort.ReadTimeout = 1000;
        _serialPort.WriteTimeout = 1000;

        try
        {
            _serialPort.Open();
            _serialPort.WriteLine("'sup");
            string response = _serialPort.ReadLine();
            Debug.Log($"[SyncPulseDetector] response to greeting: '{response}'");
            _serialPort.Close();
        }
        catch(Exception ex)
        {
            Debug.Log($"[SyncPulseDetector::InitializeSerialPort] {ex.Message}");
        }
        finally
        {
            _serialPort.Close();
        }
    }

    public SyncPulseEvent DetectOnePulse()
    {
        SyncPulseEvent syncPulseEvent = new SyncPulseEvent();

        try
        {
            _serialPort.Open();

            var t0 = HighPrecisionClock.UtcNowIn100nsTicks;
            _serialPort.WriteLine($"getlast;");
            string response = _serialPort.ReadLine();
            var t3 = HighPrecisionClock.UtcNowIn100nsTicks;

            var parts = response.Split(';');
            var tlast = float.Parse(parts[1]);
            var t1 = float.Parse(parts[2]);
            var t2 = float.Parse(parts[3]);

            if (tlast > 0)
            {
                var rtt = (double)(t3 - t0)*1e-4 - (t2 - t1) * 1e-3;

                syncPulseEvent.systemTime = t3;
                syncPulseEvent.offset = (tlast - t2) / 1000;
                syncPulseEvent.rtt = rtt;
                syncPulseEvent.result = SyncPulseEvent.Result.Detected;
            }
        }
        catch (TimeoutException)
        {
            Debug.Log($"[SyncPulseDetector] Timed out");
            syncPulseEvent.result = SyncPulseEvent.Result.Error;
        }
        catch (Exception ex)
        {
            Debug.Log($"[SyncPulseDetector] {ex.Message}");
            syncPulseEvent.result = SyncPulseEvent.Result.Error;
        }
        finally
        {
            _serialPort.Close();
        }

        return syncPulseEvent;
    }

  }
