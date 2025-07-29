public class GazeCalibrationLog
{
    private static readonly int MAXSIZE = 100;
    public float[] time = new float[MAXSIZE];
    public float[] x = new float[MAXSIZE];
    public float[] y = new float[MAXSIZE];

    private int _index;

    public GazeCalibrationLog() { }

    public void Add(float t, float x, float y)
    {
        this.time[_index] = t;
        this.x[_index] = x;
        this.y[_index] = y;
        _index++;
    }

    public void Trim()
    {
        System.Array.Resize(ref this.time, _index);
        System.Array.Resize(ref this.x, _index);
        System.Array.Resize(ref this.y, _index);
    }
}
