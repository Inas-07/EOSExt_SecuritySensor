﻿using System.Collections.Generic;
using System.Linq;
using ChainedPuzzles;
using EOSExt.SecuritySensor.Component;
using EOSExt.SecuritySensor.Definition;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using FloLib.Networks.Replications;
using GTFO.API.Extensions;
using UnityEngine;
using SNetwork;

namespace EOSExt.SecuritySensor
{
    public class SensorGroup
    {
        private List<GameObject> basicSensors = new();

        private List<MovableSensor> movableSensors = new();

        public int SensorGroupIndex { get; private set; }

        public SensorGroupSettings Settings { get; private set; }

        public StateReplicator<SensorGroupState> StateReplicator { get; private set; }

        public IEnumerable<GameObject> BasicSensors => basicSensors;

        public IEnumerable<MovableSensor> MovableSensors => movableSensors; // TODO: position sync of movable

        public ActiveState State => StateReplicator?.State.status ?? ActiveState.ENABLED;

        public void ChangeToState(ActiveState status) // TODO: has undone last commit. events for changing ss state is executed, but state is not changed????
        {
            ChangeToStateUnsynced(new() { status = status });
            EOSLogger.Debug($"ChangeState: SecuritySensorGroup_{SensorGroupIndex} changed to state {status}");
            if(SNet.IsMaster)
            {
                StateReplicator?.SetState(new() { status = status });
            }
        }

        private void OnStateChanged(SensorGroupState oldState, SensorGroupState newState, bool isRecall)
        {
            EOSLogger.Warning($"OnStateChanged: isRecall ? {isRecall}");

            if (isRecall)
            {
                EOSLogger.Debug($"Recalling: SecuritySensorGroup_{SensorGroupIndex} changed to state {newState.status}");
                ChangeToStateUnsynced(newState);
            }
            else
            {
                if (oldState.status != newState.status) // synced state from host if sth went wrong on local compute
                {
                    ChangeToStateUnsynced(newState);
                }
            }
        }

        private void ChangeToStateUnsynced(SensorGroupState newState)
        {
            switch (newState.status)
            {
                case ActiveState.ENABLED:
                    basicSensors.ForEach(sensorGO => sensorGO.SetActive(true));
                    ResumeMovingMovables();
                    break;
                case ActiveState.DISABLED:
                    PauseMovingMovables();
                    basicSensors.ForEach(sensorGO => sensorGO.SetActive(false));
                    break;
            }
        }

        public static SensorGroup Instantiate(SensorGroupSettings sensorGroupSettings, int sensorGroupIndex)
        {
            SensorGroup sg = new SensorGroup();
            sg.SensorGroupIndex = sensorGroupIndex;
            sg.Settings = sensorGroupSettings;

            foreach (var sensorSetting in sensorGroupSettings.SensorGroup)
            {
                var position = sensorSetting.Position.ToVector3();
                if (position == Vector3.zeroVector) continue;

                GameObject sensorGO = null; 
                switch (sensorSetting.SensorType)
                {
                    case SensorType.BASIC:
                        sensorGO = Object.Instantiate(Assets.CircleSensor);
                        sg.basicSensors.Add(sensorGO);
                        break;
                    case SensorType.MOVABLE:
                        var movableSensor = MovableSensor.Instantiate(sensorSetting);
                        if(movableSensor == null)
                        {
                            EOSLogger.Error($"ERROR: failed to build movable sensor");
                            continue;
                        }
                        sensorGO = movableSensor.movableGO;

                        sg.movableSensors.Add(movableSensor);
                        break;
                    default:
                        EOSLogger.Error($"Unsupported SensorType {sensorSetting.SensorType}, skipped");
                        continue;
                }

                sensorGO.transform.SetPositionAndRotation(position, Quaternion.identityQuaternion);

                float height = 0.6f / 3.7f;
                sensorGO.transform.localScale = new Vector3(sensorSetting.Radius, sensorSetting.Radius, sensorSetting.Radius);
                sensorGO.transform.localPosition += Vector3.up * height;

                var sensorCollider = sensorGO.AddComponent<SensorCollider>();
                sensorCollider.Setup(sensorSetting, sg);
                sensorGO.SetActive(true);
            }

            uint allotedID = EOSNetworking.AllotReplicatorID();
            if (allotedID == EOSNetworking.INVALID_ID)
            {
                EOSLogger.Error($"SensorGroup.Instantiate: replicator ID depleted, cannot create StateReplicator...");
            }
            else
            {
                sg.StateReplicator = StateReplicator<SensorGroupState>.Create(allotedID, new() { status = ActiveState.ENABLED }, LifeTimeType.Level);
                sg.StateReplicator.OnStateChanged += sg.OnStateChanged;
            }

            return sg;
        }

        public void StartMovingMovables() => movableSensors.ForEach(movable => movable.StartMoving());

        public void PauseMovingMovables() => movableSensors.ForEach(movable => movable.PauseMoving());

        public void ResumeMovingMovables() => movableSensors.ForEach(movable => movable.ResumeMoving());

        public void Destroy()
        {
            basicSensors.ForEach(Object.Destroy);
            basicSensors.Clear();
            movableSensors.ForEach(m => m.Destroy());
            movableSensors.Clear();
            StateReplicator = null;
            Settings = null;
        }

        private SensorGroup() { }
    }
}
