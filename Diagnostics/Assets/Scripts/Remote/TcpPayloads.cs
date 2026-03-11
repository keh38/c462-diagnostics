using Newtonsoft.Json;

namespace HTS.Tcp
{
    [JsonObject]
    public class ConnectionRequestPayload
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }

    [JsonObject]
    public class ConnectionResponsePayload
    {
        public string HostName { get; set; }
        public string VersionNumber { get; set; }
        public string SceneName { get; set; }
    }

    [JsonObject]
    public class ChangeScenePayload
    {
        public string SceneName { get; set; }
    }

    [JsonObject]
    public class RecordPayload
    {
        public string Path { get; set; }
    }

    [JsonObject]
    public class DataStreamStatusPayload
    {
        public int Status { get; set; }
    }

    [JsonObject]
    public class ClockSyncPayload
    {
        public long T1 { get; set; }
        public long T2 { get; set; }
    }

    [JsonObject]
    public class TextFilePayload
    {
        public string Filename { get; set; }
        public string Content { get; set; }
    }

    [JsonObject]
    public class ErrorPayload
    {
        public string Message { get; set; }
    }

    [JsonObject]
    public class FilenamePayload
    {
        public string Filename { get; set; }
    }

    [JsonObject]
    public class TransferFilePayload
    {
        public string Folder { get; set; }
        public string Filename { get; set; }
        public string Content { get; set; }
    }
}
