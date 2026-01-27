using UnityEngine;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

public class SRIFixationPoint : MonoBehaviour
{
/*    public SpriteRenderer vertical;
    public SpriteRenderer horizontal;
    public SpriteRenderer circle;

    private float worldPerPixel;

    void Awake()
    {
        var yWorld = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize * 2;
#if UNITY_EDITOR
        var sz = KLib.Unity.GetMainGameViewSize();
        worldPerPixel = yWorld / sz.y;
#else
        worldPerPixel = yWorld / Screen.height;
#endif
    }

    void Start()
    {
        Hide();
    }

    public void Hide()
    {
        transform.localPosition = new Vector2(-5, 0);
    }

    public void ShowCross()
    {
        NGUITools.SetActive(vertical.gameObject, true);
        NGUITools.SetActive(horizontal.gameObject, true);
        NGUITools.SetActive(circle.gameObject, false);

        transform.localPosition = Vector2.zero;
    }

    public void ShowCircle()
    {
        NGUITools.SetActive(vertical.gameObject, false);
        NGUITools.SetActive(horizontal.gameObject, false);
        NGUITools.SetActive(circle.gameObject, true);

        transform.localPosition = Vector2.zero;
    }

    public void Initialize(Color color, int size)
    {
        float sizeWorld = worldPerPixel * size;

        vertical.color = color;
        vertical.transform.localScale = new Vector3(sizeWorld / 3, sizeWorld, 1);

        horizontal.color = color;
        horizontal.transform.localScale = new Vector3(sizeWorld, sizeWorld / 3, 1);

        circle.color = color;
        circle.transform.localScale = new Vector3(sizeWorld, sizeWorld, 1);
    }
*/
}
 