using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using GTFO.API;
using EOSExt.SecuritySensor.Definition;
using UnityEngine;
using EOSExt.SecuritySensor.Component;
using ChainedPuzzles;
using System;
using GameData;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using GTFO.API.Utilities;
using UnityEngine.UI;
using TMPro;
using SNetwork;

namespace EOSExt.SecuritySensor
{
    public enum SensorEventType
    {
        ToggleSensorGroupState = 400,
        ToggleAllSensorGroups = 401,
    }

    public sealed class SecuritySensorManager : GenericExpeditionDefinitionManager<SensorGroupSettings>
    {
        public static SecuritySensorManager Current { get; private set; } = new();

        private List<SensorGroup> sensorGroups = new();

        internal static readonly SensorSync SyncTrigger = new();

        protected override string DEFINITION_NAME => "SecuritySensor";

        public void BuildSensorGroup(SensorGroupSettings sensorGroupSettings)
        {
            int groupIndex = sensorGroupSettings.Index == uint.MaxValue ? sensorGroups.Count : (int)sensorGroupSettings.Index;
            var sg = SensorGroup.Instantiate(sensorGroupSettings, groupIndex);
            sensorGroups.Add(sg);

            EOSLogger.Debug($"SensorGroup_{groupIndex} built");
        }

        private SensorGroup FindSensorGroup(int groupIndex) => sensorGroups.Find(sg => sg.SensorGroupIndex == groupIndex);

        internal void TriggerSensor(int groupIndex)
        {
            var sg = FindSensorGroup(groupIndex);
            if (sg == null)
            {
                EOSLogger.Error($"TriggerSensor: invalid SensorGroup index {groupIndex}");
                return;
            }
            //// MovableSensor would trigger `SensorCollider` cuz its game object is not (and cannot) set to inactive
            //// So do some special case handling here
            //if (sg.sensorGroup.StateReplicator.State.status != ActiveState.ENABLED)
            //{
            //    return; 
            //}

            sg.Settings
                .EventsOnTrigger
                .ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, e.Trigger, true));
            EOSLogger.Warning($"TriggerSensor: SensorGroup_{groupIndex} triggered");
        }

        private void BuildSecuritySensor()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(BuildSensorGroup);
        }

        private void Clear()
        {
            sensorGroups.ForEach(sg => sg.Destroy());
            sensorGroups.Clear();
        }

        private void ToggleSensorGroup(WardenObjectiveEventData e)
        {
            int groupIndex = e.Count;
            var sg = FindSensorGroup(groupIndex);
            if (sg == null)
            {
                EOSLogger.Error($"ToggleSensorGroup: invalid SensorGroup index {groupIndex}");
                return;
            }

            sg.ChangeToState(e.Enabled ? ActiveState.ENABLED : ActiveState.DISABLED);
        }

        private void ToggleAllSensorGroups(WardenObjectiveEventData e)
        {
            ActiveState state = e.Enabled ? ActiveState.ENABLED : ActiveState.DISABLED;
            sensorGroups.ForEach(sg => sg.ChangeToState(state));
        }

        private SecuritySensorManager() : base()
        {
            LevelAPI.OnBuildStart += () => { Clear(); BuildSecuritySensor(); };
            LevelAPI.OnLevelCleanup += Clear;

            EventAPI.OnExpeditionStarted += () => {
                sensorGroups.ForEach(sg => sg.StartMovingMovables());
            };

            EOSWardenEventManager.Current.AddEventDefinition(SensorEventType.ToggleSensorGroupState.ToString(), (int)SensorEventType.ToggleSensorGroupState, ToggleSensorGroup);
            EOSWardenEventManager.Current.AddEventDefinition(SensorEventType.ToggleAllSensorGroups.ToString(), (int)SensorEventType.ToggleAllSensorGroups, ToggleAllSensorGroups);
        }

        static SecuritySensorManager()
        {
            if (!SyncTrigger.IsSetup) SyncTrigger.Setup();
        }
    }
}
