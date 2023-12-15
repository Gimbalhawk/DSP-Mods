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
        public const string pluginVersion = "1.6.0";

        public static Item Drone = new Item(){ Id = 5001, Name = "Drone" };
        public static Item Vessel = new Item(){ Id = 5002, Name = "Vessel" };
        public static Item Bot = new Item(){ Id = 5003, Name = "Bot" };

        private static StorageUtility StorageUtility { get; set; }
        private static BuildMenuUIAddOn UI { get; set; }

        public void Awake()
		{
            ModConfig.InitConfig(Config);

            StorageUtility = new StorageUtility();

            Harmony.CreateAndPatchAll(typeof(AutoLogisticsDroneSetup));
            Harmony.CreateAndPatchAll(typeof(ModConfigWindow));

            DSP_Mods.Logger.Init(Logger, "AutoLogisticsDroneSetup");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.NewStationComponent))]
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

            if (station.isStellar)
            {
				DSP_Mods.Logger.Instance.LogInfo("Interstellar logistics station built");
                if (ModConfig.DepositDronesILS.Value)
				{
                    AddDrones(station, _desc, StorageUtility, ModConfig.DroneAmountILS.Value);
                }

                if (ModConfig.DepositVesselsILS.Value)
                {
                    AddShips(station, _desc, StorageUtility, ModConfig.VesselAmountILS.Value);
                }
            }
            else
            {
				DSP_Mods.Logger.Instance.LogInfo("Planetary logistics station built");
				if (ModConfig.DepositDronesPLS.Value)
                {
                    AddDrones(station, _desc, StorageUtility, ModConfig.DroneAmountPLS.Value);
                }
            }
        }

		[HarmonyPostfix, HarmonyPatch(typeof(PlanetTransport), "NewDispenserComponent")]
		public static void NewDispenserComponent_PostFix(PlanetTransport __instance, int _entityId, int _pcId, PrefabDesc _desc)
        {
			var player = GameMain.mainPlayer;
			if (player == null)
			{
				DSP_Mods.Logger.Instance.LogInfo("Couldn't find player");
				return;
			}

			var entity = __instance.factory.GetEntityData(_entityId);
			var dispenser = __instance.dispenserPool[entity.dispenserId];
			if (dispenser == null)
			{
				DSP_Mods.Logger.Instance.LogInfo("Couldn't find logistic dispenser? That shouldn't happen");
				return;
			}

			DSP_Mods.Logger.Instance.LogInfo("Logistics dispenser built");
            
            if (ModConfig.DepositBots.Value)
            {
                AddBots(dispenser, _desc, StorageUtility, ModConfig.BotAmount.Value);
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

			if (desiredAmount <= 0)
				return;

			var drones = util.RemoveItems(Drone, desiredAmount);
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

			if (desiredAmount <= 0)
				return;

			var ships = util.RemoveItems(Vessel, desiredAmount);
            if (ships <= 0)
                return;

            station.idleShipCount = ships;
		}

		private static void AddBots(DispenserComponent dispenser, PrefabDesc desc, StorageUtility util, int desiredAmount)
        {
            if (dispenser == null || desc == null) 
                return;

            var botMax = desc.dispenserMaxCourierCount;
            if (botMax <= 0)
                return;

            if (desiredAmount < 0 || desiredAmount > botMax)
                desiredAmount = botMax;

            if (desiredAmount <= 0)
                return;

            var bots = util.RemoveItems(Bot, desiredAmount);
            if (bots <= 0)
                return;

            dispenser.idleCourierCount = bots;
        }

		[HarmonyPostfix, HarmonyPatch(typeof(UIBuildMenu), nameof(UIBuildMenu.OnCategoryButtonClick))]
		public static void OnCategoryButtonClick_Postfix(UIBuildMenu __instance)
        {
            var uiBuildMenu = __instance;

            // We only want to modify the logistics buildings category
            if (uiBuildMenu.currentCategory != 6)
            {
                if (UI != null)
                {
                    UI.Hide();
                }

                return;
            }

            if (UI == null)
            {
                UI = BuildMenuUIAddOn.InitUI(uiBuildMenu);
            }

            if (UI != null)
            {
                UI.Show();
            }
        }

        public void OnGUI()
        {
            if (ModConfigWindow.Visible)
            {
                ModConfigWindow.OnGUI();
            }
        }
    }
}
