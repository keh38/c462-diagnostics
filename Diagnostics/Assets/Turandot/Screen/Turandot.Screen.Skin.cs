using UnityEngine;

namespace Turandot.Screen
{
    public class Skin
    {
        public Color screenColor;
        public Color baseColor;
        public Color buttonDownColor;
        public Color buttonTextColor;
        public Color sliderBackgroundColor;
        public Color sliderForegroundColor;
        public Color progressForegroundColor;
        public Color helpBackgroundColor;
        public Color helpTextColor;
        public Color messageColor;

        public Skin() { }

        public static Skin PupilSkin
        {
            get { return GetPupilSkin(); }
        }

        public static Skin DefaultSkin
        {
            get { return GetDefaultSkin(); }
        }


        private static Skin GetDefaultSkin()
        {
            Skin skin = new Skin();

            skin.screenColor = new Color(208f / 255, 208f / 255, 208f / 255);
            skin.baseColor = new Color(0.357f, 0.537f, 0.357f);
            skin.buttonDownColor = new Color(253f / 255, 224f / 255, 157f / 255);
            skin.buttonTextColor = Color.white;
            skin.sliderBackgroundColor = skin.screenColor;
            skin.sliderForegroundColor = new Color(60f / 255, 90f / 255, 60f / 255);
            skin.progressForegroundColor = new Color(60f / 255, 90f / 255, 60f / 255);
            skin.helpBackgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
            skin.helpTextColor = Color.black;
            skin.messageColor = new Color(0f, 0.6f, 0f);

            return skin;
        }

        private static Skin GetPupilSkin()
        {
            Skin skin = new Skin();

            skin.screenColor = new Color(0.5f, 0.5f, 0.5f);
            skin.baseColor = new Color(81f / 255, 81f / 255, 81f / 255);
            skin.buttonDownColor = new Color(0.15f, 0.15f, 0.15f);
            skin.buttonTextColor = new Color(208f / 255, 197f / 255, 197f / 255);
            skin.sliderBackgroundColor = skin.screenColor;
            skin.sliderForegroundColor = Color.black;
            skin.progressForegroundColor = new Color(110f / 255, 110f / 255, 110f / 255);
            skin.helpBackgroundColor = new Color(0.65f, 0.65f, 0.65f, 0.8f);
            skin.helpTextColor = Color.black;
            skin.messageColor = new Color(0.1961f, 0.9922f, 0.0431f);

            return skin;
        }

    }
}