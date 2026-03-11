using KLibU.Net;

public interface IRemoteControllable
{
    TcpMessage ProcessRPC(TcpMessage request);
    void ChangeScene(string newScene);
}