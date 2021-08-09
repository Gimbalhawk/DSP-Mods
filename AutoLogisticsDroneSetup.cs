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
        public const string pluginVersion = "1.1.0";

        public const int DRONE_ITEMID = 5001;
        public const int VESSEL_ITEMID = 5002;
        public const int PLS_ITEMID = 2103;
        public const int ILS_ITEMID = 2104;
        

        private static ConfigEntry<bool> DepositDronesPLS;
        private static ConfigEntry<bool> DepositDronesILS;
        private static ConfigEntry<bool> DepositVesselsILS;

        private static ManualLogSource s_logger;

        public void Awake()
		{
            AutoLogisticsDroneSetup.DepositDronesPLS = Config.Bind<bool>("Planetary Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Planetary Logistics Station");
            AutoLogisticsDroneSetup.DepositDronesILS = Config.Bind<bool>("Interstellar Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Interstellar Logistics Station");
            AutoLogisticsDroneSetup.DepositVesselsILS = Config.Bind<bool>("Interstellar Logistics", "Deposit Vessels", true, "Whether vessels should automatically be added to a newly created Interstellar Logistics Station");

            Harmony.CreateAndPatchAll(typeof(AutoLogisticsDroneSetup));
            
            s_logger = Logger;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlanetTransport), "NewStationComponent")]
        public static void NewStationComponent_PostFix(PlanetTransport __instance, int _entityId, int _pcId, PrefabDesc _desc)
		{
            var player = GameMain.mainPlayer;
            if (player == null)
			{
                LogInfo("Couldn't find player");
                return;
			}

            var entity = __instance.factory.GetEntityData(_entityId);
            var station = __instance.stationPool[entity.stationId];
            if (station == null)
			{
                LogInfo("Couldn't find station? That shouldn't happen");
                return;
			}

            if (station.isCollector)
                return;

            if (station.isStellar)
            {
                if (DepositDronesILS.Value)
                {
                    AddDrones(station, player, _desc);
                }

                if (DepositVesselsILS.Value)
                {
                    AddShips(station, player, _desc);
                }
            }
            else
            {
                if (DepositDronesPLS.Value)
                {
                    AddDrones(station, player, _desc);
                }
            }
        }

        private static void AddDrones(StationComponent station, Player player, PrefabDesc desc)
		{
            if (station == null || player == null || desc == null)
                return;

            var droneMax = desc.stationMaxDroneCount;
            if (droneMax <= 0)
                return;

            var drones = RemoveItem(player, droneMax, DRONE_ITEMID);
            if (drones <= 0)
                return;

            station.idleDroneCount = drones;
        }

        private static void AddShips(StationComponent station, Player player, PrefabDesc desc)
        {
            if (station == null || player == null || desc == null)
                return;

            var shipMax = desc.stationMaxShipCount;
            if (shipMax <= 0)
                return;

            var ships = RemoveItem(player, shipMax, VESSEL_ITEMID);
            if (ships <= 0)
                return;

            station.idleShipCount = ships;
        }

        private static int RemoveItem(Player player, int count, int itemId)
		{
            if (player == null || player.package == null)
                return 0;

            var realCount = player.package.TakeItem(itemId, count);
            return realCount;
		}

        private static void LogInfo(string msg)
		{
            s_logger.LogInfo("AutoLogisticsDroneSetup : {msg}");
		}
    }
}
