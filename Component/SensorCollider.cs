using EOSExt.SecuritySensor.Definition;
using Player;
using SNetwork;
using UnityEngine;

namespace EOSExt.SecuritySensor.Component
{
    public class SensorCollider : MonoBehaviour
    {
        public static float CHECK_INTERVAL { get; } = 0.1f;

        private float nextCheckTime = float.NaN;

        private SensorSettings settings;

        private Vector3 Position => gameObject.transform.position;

        private int last_playersInSensor = 0;

        public SensorGroup Parent { get; internal set; }

        internal void Setup(SensorSettings sensorSettings, SensorGroup parent)
        {
            this.settings = sensorSettings;
            this.Parent = parent;
        }

        void Update()
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            if (!float.IsNaN(nextCheckTime) && Clock.Time < nextCheckTime) return;
            nextCheckTime = Clock.Time + CHECK_INTERVAL;
            if (Parent.State != ActiveState.ENABLED) return;

            int current_playersInSensor = 0; 
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                if (player.Owner.IsBot || !player.Alive) continue;

                if((this.Position - player.Position).magnitude < settings.Radius)
                {
                    current_playersInSensor++;
                }
            }

            if (current_playersInSensor > last_playersInSensor)
            {
                SecuritySensorManager.SyncTrigger.Send(Parent.SensorGroupIndex, null, SNet_ChannelType.GameOrderCritical);
            }

            last_playersInSensor = current_playersInSensor;
        }

        void OnDestroy()
        {
            Parent = null;
            settings = null;
        }
    }
}
