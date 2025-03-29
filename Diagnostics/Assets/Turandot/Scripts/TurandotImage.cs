using UnityEngine;
using System.Collections;
using System.IO;

using Turandot.Cues;

namespace Turandot.Scripts
{
    public class TurandotImage : MonoBehaviour
    {
        public GameObject frame;

        private Image _image;
        private string _lastFile = "";
        private Texture2D _tex2D = null;

        public void Initialize() { }

        public void Activate(Image image)
        {
            _image = image;

            if (!_image.BeginVisible || string.IsNullOrEmpty(image.filename))
                return;

            if (string.IsNullOrEmpty(_lastFile) || image.filename != _lastFile)
            {
                string pp = "";
                //string pp = KLib.FileIO.CombinePaths(DataFileLocations.LocalResourceFolder("Images"), System.IO.Path.GetFileName(image.filename));
#if UNITY_EDITOR
                pp = @"C:/" + pp;
#endif

                //                _tex2D.LoadImage(File.ReadAllBytes(pp));

                WWW www = new WWW("file://" + pp.Replace(@"\", "/"));
                while (!www.isDone) { }

                //if (_tex2D == null)
                //{
                //    _tex2D = new Texture2D(10, 10);
                //}
                //www.LoadImageIntoTexture(_tex2D);
                frame.GetComponent<Renderer>().material.mainTexture = www.texture;
                _tex2D = www.texture;

                _lastFile = image.filename;

                Vector3[] vnorm = new Vector3[4];
                vnorm[0] = new Vector3(0f, 0f, 0);
                vnorm[1] = new Vector3(0f, 1, 0);
                vnorm[2] = new Vector3(1, 1, 0);
                vnorm[3] = new Vector3(1, 0f, 0);

                Mesh mesh = null;//KLib.Polygon.GeneratePolygonalMesh(vnorm);

                Vector3[] v = new Vector3[mesh.vertexCount];
                Vector2[] uv = new Vector2[mesh.vertexCount];

                for (int k = 0; k < mesh.vertexCount; k++)
                {
                    v[k] = mesh.vertices[k];
                    if (v[k].z > 0)
                    {
                        uv[k] = new Vector2(v[k].x, v[k].y);
                    }
                    else
                    {
                        uv[k] = new Vector2(-1, -1);
                    }
                    v[k].x = _tex2D.width * (mesh.vertices[k].x - 0.5f);
                    v[k].y = _tex2D.height * (mesh.vertices[k].y - 0.5f);
                    v[k].z = 0.05f * mesh.vertices[k].z;
                }

                mesh.vertices = v;
                mesh.uv = uv;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                frame.GetComponent<MeshFilter>().mesh = mesh;
            }

            int width = _tex2D.width;
            int height = _tex2D.height;
            int x = image.X;
            if (image.horizontalAlignment == Image.HorizontalAlignment.Left)
                x += width / 2;
            else if (image.horizontalAlignment == Image.HorizontalAlignment.Right)
                x -= width / 2;

            int y = image.Y;
            if (image.verticalAlignment == Image.VerticalAlignment.Top)
                y -= height / 2;
            else if (image.verticalAlignment == Image.VerticalAlignment.Bottom)
                y += height / 2;

            transform.localPosition = new Vector2(x, y);
        }

        public void Deactivate()
        {
            if (_image != null && _image.EndVisible)
            {

            }
            else
            {
                HideCue();
            }
        }

        public void HideCue()
        {
            transform.localPosition = new Vector2(-2500, 0);
        }

    }
}