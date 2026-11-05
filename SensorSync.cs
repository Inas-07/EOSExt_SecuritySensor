using AmorLib.Networking;

namespace EOSExt.SecuritySensor
{
    /// <summary>
    /// Sends the "sensor group triggered" packet to every client, so the group events run on all of
    /// them instead of only on the client that happened to detect the player.
    /// </summary>
    internal sealed class SensorSync : SyncedEvent<int>
    {
        public override string GUID => "EOS-SensorTrigger";

        protected override void Receive(int packet)
        {
            SecuritySensorManager.Current.TriggerSensor(packet);
        }

        protected override void ReceiveLocal(int packet)
        {
            Receive(packet);
        }
    }
}
