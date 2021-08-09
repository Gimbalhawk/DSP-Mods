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
        public const string pluginVersion = "1.0.2";

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

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), "NotifyBuilt")]
        public static void NotifyBuilt_Postfix(ref PlayerAction_Build __instance, int preObjId, int postObjId)
        {
            if (__instance == null)
            {      
                s_logger.LogInfo("Instance null?");
				return;
			}

            var factory = __instance.factory;
            if (factory == null)
            {
                s_logger.LogInfo("Factory null?");
                return;
            }

            var entity = __instance.factory.GetEntityData(postObjId);
            var stationId = entity.stationId;
            var player = __instance.player;

            if (player == null)
            {
                s_logger.LogInfo("Player null?");
                return;
            }

            var transport = factory.transport;
            if (transport == null)
            {
                s_logger.LogInfo("Transport null?");
                return;
            }

            var station = __instance.factory.transport.stationPool[stationId];
            // We don't care about structures that aren't stations
            if (station == null)
            {
                s_logger.LogInfo("Station null?");
                return;
            }

            s_logger.LogInfo("Constructed station");

            // Collector stations don't have drones so we don't care about them
            if (station.isCollector)
                return;

            if (station.isStellar)
            {
				if (DepositDronesILS.Value)
				{
                    AddDrones(station, player, ILS_ITEMID);
                }

                if (DepositVesselsILS.Value)
                {
                    AddShips(station, player, ILS_ITEMID);
                }
            }
            else
            {
                if (DepositDronesPLS.Value)
                {
                    AddDrones(station, player, PLS_ITEMID);
                }
            }
		}

        private static void AddDrones(StationComponent station, Player player, int stationTypeId)
		{
            var desc = GetDesc(stationTypeId);
            if (desc == null)
                return;

            var droneMax = desc.stationMaxDroneCount;
            if (droneMax <= 0)
                return;

            var drones = RemoveItem(player, droneMax, DRONE_ITEMID);
            if (drones <= 0)
                return;

            station.idleDroneCount = drones;
        }

        private static void AddShips(StationComponent station, Player player, int stationTypeId)
        {
            var desc = GetDesc(stationTypeId);
            if (desc == null)
                return;

            var shipMax = desc.stationMaxShipCount;
            if (shipMax <= 0)
                return;

            var ships = RemoveItem(player, shipMax, VESSEL_ITEMID);
            if (ships <= 0)
                return;

            station.idleShipCount = ships;
        }

        private static PrefabDesc GetDesc(int id)
		{
            var item = LDB.items.Select(id);
            if (item == null)
                return null;

            var desc = item.prefabDesc;
            if (desc == null)
                s_logger.LogInfo("Couldn't locate station description");

            return desc;
        }

        private static int RemoveItem(Player player, int count, int itemId)
		{
            if (player == null || player.package == null)
                return 0;

            var realCount = player.package.TakeItem(itemId, count);
            return realCount;
		}
    }
}
