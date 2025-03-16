using System;
using System.Collections.Generic;
using System.IO;

using Audiograms;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Calibration
{
    public class CalibrationFactory
    {
        public static string DefaultFolder = @"C:\Users\Public\Music\{4CF46EAB-0304-4429-9666-035ADFDB847F}\{18AE38D6-5684-4966-9047-C49547486142}\Calibration";
        public static string AudiogramFolder = "";

        public static CalibrationData Load(LevelUnits refMode, string transducer, string destination, float maxLevelMargin = 0)
        {
            CalibrationData result = null;
            AcousticCalibration acal = null;

            if (refMode == LevelUnits.dB_attenuation || refMode == LevelUnits.Volts)
            {
                return CalibrationData.Create_dBAtten();
            }
            else if (refMode == LevelUnits.dB_Vrms)
            {
                result = CalibrationData.Create_dBVrms(acal);
            }

            acal = AcousticCalibration.Load(DefaultFolder, transducer, destination);

            if (refMode == LevelUnits.dB_SPL && acal != null)
            {
                result = new CalibrationData(acal);
            }

            if (refMode == LevelUnits.dB_SPL_noLDL && acal != null)
            {
                result = new CalibrationData(acal);
            }

            if (refMode == LevelUnits.dB_HL && acal!=null)
            {
                result = CalibrationData.Create_dBHL(acal);
            }

            if ((refMode == LevelUnits.dB_SL || refMode == LevelUnits.PercentDR) && acal!=null)
            {
                Audiogram a = null;

                AudiogramData audiograms = null;
#if FIXME
                if (!string.IsNullOrEmpty(AudiogramFolder))
                    audiograms = AudiogramData.LoadPC(AudiogramFolder, "agram.xml");
                else if (File.Exists(DataFileLocations.AudiogramPath))
                    audiograms = AudiogramData.Load(DataFileLocations.AudiogramPath);
#endif

                if (audiograms != null)
                {
                    a = audiograms.Get(destination);
                    result = CalibrationData.Create_dBSL(a, acal);
                }
                else
                {
                    result = CalibrationData.Create_dBHL(acal);
                }
            }
            if (result != null && acal != null && refMode != LevelUnits.dB_SPL_noLDL)
            {
                Audiograms.Audiogram ldl = null;

                AudiogramData LDLs = null;
                if (!string.IsNullOrEmpty(DefaultFolder))
                    LDLs = AudiogramData.LoadPC(DefaultFolder, "ldlgram.xml");
#if FIXME
                else if (File.Exists(DataFileLocations.LDLPath))
                    LDLs = AudiogramData.Load(DataFileLocations.LDLPath);
#endif
                if (LDLs != null)
                {
                    if (refMode == LevelUnits.PercentDR || refMode == LevelUnits.dB_SPL) LDLs.ReplaceNaNWithMax(transducer);
                    ldl = LDLs.Get(destination);
                }

                if (ldl != null)
                {
                    result.SetUpperBounds(acal, ldl, maxLevelMargin);
                    if (refMode == LevelUnits.PercentDR)
                    {
                        Audiograms.Audiogram a = null;

                        AudiogramData audiograms = null;
                        if (!string.IsNullOrEmpty(AudiogramFolder))
                            audiograms = AudiogramData.LoadPC(AudiogramFolder, "agram.xml");
#if FIXME
                        else if (File.Exists(DataFileLocations.AudiogramPath))
                            audiograms = AudiogramData.Load(DataFileLocations.AudiogramPath);
#endif
                        a = audiograms.Get(destination);
                        result.ComputeDynamicRange(a, ldl);
                    }

                }
            }

            if (result != null)
            {
                result.name = destination;
            }

            return result;
        }
    }

}