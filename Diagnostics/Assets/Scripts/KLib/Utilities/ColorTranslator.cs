using System;
using System.ComponentModel;

namespace KLib
{
    public static class ColorTranslator
    {
        public static UnityEngine.Color WindowsToUnity(System.Drawing.Color windowsColor)
        {
            //UnityEngine.Debug.Log($"windows [{windowsColor.R}, {windowsColor.G}, {windowsColor.B}, {windowsColor.A}]");
            return ColorFromARGB((uint)windowsColor.ToArgb());
        }

        public static System.Drawing.Color UnityToWindows(UnityEngine.Color unityColor)
        {
            return System.Drawing.Color.FromArgb(ColorInt(unityColor));
        }

        public static int ColorInt(float r, float g, float b, float a)
        {
            int val = ((int)(b * 255));
            val += ((int)(g * 255) << 8);
            val += ((int)(b * 255) << 16);
            val += ((int)(a * 255) << 24);

            return val;
        }

        public static int ColorInt(UnityEngine.Color color)
        {
            return ColorInt(color.a, color.g, color.b, color.a);
        }

        public static UnityEngine.Color ColorFromString(string colorString)
        {
            var parts = colorString.Split(',');
            float r = float.Parse(parts[0]) / 255f;
            float g = float.Parse(parts[1]) / 255f;
            float b = float.Parse(parts[2]) / 255f;
            return new UnityEngine.Color(r, g, b);
        }

        public static UnityEngine.Color ColorFromFloatString(string colorString)
        {
            var parts = colorString.Split(',');
            float r = float.Parse(parts[0]);
            float g = float.Parse(parts[1]);
            float b = float.Parse(parts[2]);
            return new UnityEngine.Color(r, g, b);
        }

        public static UnityEngine.Color ColorFromARGB(int color)
        {
            return new UnityEngine.Color(ColorIntToRed((uint)color), ColorIntToGreen((uint)color), ColorIntToBlue((uint)color), 1.0f);
        }

        public static UnityEngine.Color ColorFromARGB(uint color)
        {
            return new UnityEngine.Color(ColorIntToRed(color), ColorIntToGreen(color), ColorIntToBlue(color), ColorIntToAlpha(color));
        }

        private static float ColorIntToAlpha(uint color)
        {
            return ((color & 0xFF000000) >> 32) / 255f;
        }

        private static float ColorIntToRed(uint color)
        {
            return ((color & 0xFF0000) >> 16) / 255f;
        }

        private static float ColorIntToGreen(uint color)
        {
            return ((color & 0x00FF00) >> 8) / 255f;
        }

        private static float ColorIntToBlue(uint color)
        {
            return (color & 0xFF) / 255f;
        }

        public static string ColorToBBCode(UnityEngine.Color color)
        {
            string code = "[";
            for (int k = 0; k < 3; k++)
            {
                code += ((int)(255 * color[k])).ToString("X2");
            }

            code += "]";
            return code;
        }

    }
}