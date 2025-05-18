using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using Turandot.Cues;
using Turandot.Screen;
using Unity.VisualScripting;

namespace Turandot.Scripts
{
    public class TurandotImage : TurandotCue
    {
        [SerializeField] private Image _image;

        private ImageAction _imageAction;
        private ImageLayout _layout;

        public override string Name { get { return _layout.Name; } }
        public void Initialize(ImageLayout layout)
        {
            _layout = layout;
            LayoutControl();
            base.Initialize();
        }
        private void LayoutControl()
        {
            SetPosition(_layout.X, _layout.Y);
        }

        public override void Activate(Cue cue)
        {
            _imageAction = cue as ImageAction;

            base.Activate(cue);

            if (!_imageAction.BeginVisible || string.IsNullOrEmpty(_imageAction.Filename))
                return;


            if (!string.IsNullOrEmpty(_imageAction.Filename))
            {
                string imagePath = Path.Combine(FileLocations.LocalResourceFolder("Images"), _imageAction.Filename);

                var texture = new Texture2D(10, 10);
                texture.LoadImage(File.ReadAllBytes(imagePath));
                _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                _image.SetNativeSize();
            }

        }

    }
}