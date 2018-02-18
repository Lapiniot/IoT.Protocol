using System.Json;

namespace IoT.Protocol.Lumi.Interfaces
{
    public interface ILumiEventListener
    {
        void OnReportMessage(string sid, JsonObject data, JsonObject message);

        void OnHeartbeatMessage(string sid, JsonObject data, JsonObject message);
    }
}