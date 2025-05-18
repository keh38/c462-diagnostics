using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using Turandot.Cues;
using Turandot.Screen;
using Unity.VisualScripting;
using UnityEngine.Video;

namespace Turandot.Scripts
{
    public class TurandotVideo : TurandotCue
    {
        [SerializeField] private VideoPlayer _player;
        [SerializeField] private Camera _camera;

        private VideoAction _videoAction;
        private VideoLayout _layout;

        public override string Name { get { return _layout.Name; } }
        public void Initialize(VideoLayout layout)
        {
            _layout = layout;
            LayoutControl();
            base.Initialize();
        }
        private void LayoutControl()
        {
            float x = _layout.X - _layout.Width / 2;
            float y = _layout.Y - _layout.Height / 2;

            _camera.rect = new Rect(x, y, _layout.Width, _layout.Height); 
        }

        public override void Activate(Cue cue)
        {
            _videoAction = cue as VideoAction;

            base.Activate(cue);

            if (!_videoAction.BeginVisible || string.IsNullOrEmpty(_videoAction.Filename))
                return;

            if (!string.IsNullOrEmpty(_videoAction.Filename))
            {
                string videoPath = Path.Combine(FileLocations.LocalResourceFolder("Videos"), _videoAction.Filename);
                _player.url = videoPath;
                _player.Play();
            }

        }

    }
}