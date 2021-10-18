using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DSP_Mods
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class AutoLogisticsDroneSetup : BaseUnityPlugin
    {
        public const string pluginGuid = "gimbalhawk.dsp.autoshuttlesetup";
        public const string pluginName = "AutoShuttleSetup";
        public const string pluginVersion = "1.4.0";

        public const int DRONE_ITEMID = 5001;
        public const int VESSEL_ITEMID = 5002;
        public const int PLS_ITEMID = 2103;
        public const int ILS_ITEMID = 2104;
        
        private static ConfigEntry<int> InventoryPriority;
        private static ConfigEntry<int> StoragePriority;
        private static ConfigEntry<int> StationsPriority;

        private static ConfigEntry<bool> DepositDronesPLS;
        private static ConfigEntry<bool> DepositDronesILS;
        private static ConfigEntry<bool> DepositVesselsILS;
        private static ConfigEntry<int> DroneAmountPLS;
        private static ConfigEntry<int> DroneAmountILS;
        private static ConfigEntry<int> VesselAmountILS;

        public void Awake()
		{
            AutoLogisticsDroneSetup.InventoryPriority = Config.Bind<int>("Storage", "Inventory Priority", 1, "The priority of selecting from the player inventory. Lower numbers take precendence. Set to a negative number to disable");
            AutoLogisticsDroneSetup.StoragePriority = Config.Bind<int>("Storage", "Storage Priority", 2, "The priority of selecting from storage chests. Lower numbers take precendence. Set to a negative number to disable");
            AutoLogisticsDroneSetup.StationsPriority = Config.Bind<int>("Storage", "Stations Priority", 3, "The priority of selecting from stations. Lower numbers take precendence. Set to a negative number to disable");

            AutoLogisticsDroneSetup.DepositDronesPLS = Config.Bind<bool>("Planetary Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Planetary Logistics Station");
            AutoLogisticsDroneSetup.DroneAmountPLS = Config.Bind<int>("Planetary Logistics", "Planetary Drone Amount", -1, "How many drones should be added to a newly created Planetary Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

            AutoLogisticsDroneSetup.DepositDronesILS = Config.Bind<bool>("Interstellar Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Interstellar Logistics Station");
            AutoLogisticsDroneSetup.DroneAmountILS = Config.Bind<int>("Interstellar Logistics", "Interstellar Drone Amount", -1, "How many drones should be added to a newly created Interstellar Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

            AutoLogisticsDroneSetup.DepositVesselsILS = Config.Bind<bool>("Interstellar Logistics", "Deposit Vessels", true, "Whether vessels should automatically be added to a newly created Interstellar Logistics Station");
            AutoLogisticsDroneSetup.VesselAmountILS = Config.Bind<int>("Interstellar Logistics", "Interstellar Vessel Amount", -1, "How many vessels should be added to a newly created Interstellar Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

            Harmony.CreateAndPatchAll(typeof(AutoLogisticsDroneSetup));

            DSP_Mods.Logger.Init(Logger, "AutoLogisticsDroneSetup");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlanetTransport), "NewStationComponent")]
        public static void NewStationComponent_PostFix(PlanetTransport __instance, int _entityId, int _pcId, PrefabDesc _desc)
		{
            var player = GameMain.mainPlayer;
            if (player == null)
			{
                DSP_Mods.Logger.Instance.LogInfo("Couldn't find player");
                return;
			}

            var entity = __instance.factory.GetEntityData(_entityId);
            var station = __instance.stationPool[entity.stationId];
            if (station == null)
			{
                DSP_Mods.Logger.Instance.LogInfo("Couldn't find station? That shouldn't happen");
                return;
			}

            if (station.isCollector)
                return;

            // Read our config data to know which sources are valid
            var storageUtil = new StorageUtility();
			storageUtil.AddSource(StorageUtility.Source.Inventory, InventoryPriority.Value);
			storageUtil.AddSource(StorageUtility.Source.Storage, StoragePriority.Value);
			storageUtil.AddSource(StorageUtility.Source.Stations, StationsPriority.Value);

            if (station.isStellar)
            {
                if (DepositDronesILS.Value)
                {
                    AddDrones(station, _desc, storageUtil, DroneAmountILS.Value);
                }

                if (DepositVesselsILS.Value)
                {
                    AddShips(station, _desc, storageUtil, VesselAmountILS.Value);
                }
            }
            else
            {
                if (DepositDronesPLS.Value)
                {
                    AddDrones(station, _desc, storageUtil, DroneAmountPLS.Value);
                }
            }
        }

        private static void AddDrones(StationComponent station, PrefabDesc desc, StorageUtility util, int desiredAmount)
		{
            if (station == null || desc == null)
                return;

            var droneMax = desc.stationMaxDroneCount;
            if (droneMax <= 0)
                return;

            if (desiredAmount < 0 || desiredAmount > droneMax)
                desiredAmount = droneMax;

            var drones = util.RemoveItems(DRONE_ITEMID, desiredAmount);
            if (drones <= 0)
                return;

            station.idleDroneCount = drones;
        }

        private static void AddShips(StationComponent station, PrefabDesc desc, StorageUtility util, int desiredAmount)
        {
            if (station == null || desc == null)
                return;

            var shipMax = desc.stationMaxShipCount;
            if (shipMax <= 0)
                return;

            if (desiredAmount < 0 || desiredAmount > shipMax)
                desiredAmount = shipMax;

            var ships = util.RemoveItems(VESSEL_ITEMID, desiredAmount);
            if (ships <= 0)
                return;

            station.idleShipCount = ships;
        }
    }
}
