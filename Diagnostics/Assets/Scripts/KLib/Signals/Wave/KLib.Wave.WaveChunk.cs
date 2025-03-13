namespace KLib.Wave
{
    public class WaveChunk
    {
        public string ID = "";
        public uint size = 0;
        public WaveChunk()
        {
        }
        public WaveChunk(string id, uint size)
        {
            this.ID = id;
            this.size = size;
        }
    }
}