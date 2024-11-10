using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.JSON;
using GTFO.API;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using EOSExt.SecuritySensor.Component;

namespace EOSExt.SecuritySensor
{
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("GTFO.FloLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Inas.ExtraObjectiveSetup", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(InjectLibUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    
    public class EntryPoint: BasePlugin
    {
        public const string AUTHOR = "Inas";
        public const string PLUGIN_NAME = "EOSExt.SecuritySensor";
        public const string VERSION = "1.1.7";

        private Harmony m_Harmony;

        public override void Load()
        {
            SetupManagers();
            AssetAPI.OnAssetBundlesLoaded += Assets.Init;

            m_Harmony = new Harmony("EOSExt.SecuritySensor");
            m_Harmony.PatchAll();

            ClassInjector.RegisterTypeInIl2Cpp<SensorCollider>();
            EOSLogger.Log("ExtraObjectiveSetup.SecuritySensor loaded.");
        }

        /// <summary>
        /// Explicitly invoke Init() to all managers to eager-load, which in the meantime defines chained puzzle creation order if any
        /// </summary>
        private void SetupManagers()
        {
            SecuritySensorManager.Current.Init();
        }
    }
}

