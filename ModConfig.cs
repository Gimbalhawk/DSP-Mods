using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSP_Mods
{
	public static class ModConfig
	{
		public static ConfigEntry<int> InventoryPriority;
		public static ConfigEntry<int> StoragePriority;
		public static ConfigEntry<int> StationsPriority;

		public static ConfigEntry<bool> DepositDronesPLS;
		public static ConfigEntry<bool> DepositDronesILS;
		public static ConfigEntry<bool> DepositVesselsILS;
		public static ConfigEntry<bool> DepositBots;

		public static ConfigEntry<int> DroneAmountPLS;
		public static ConfigEntry<int> DroneAmountILS;
		public static ConfigEntry<int> VesselAmountILS;
		public static ConfigEntry<int> BotAmount;

		public static ConfigFile ConfigFile;

		public static void InitConfig(ConfigFile config)
		{
			ConfigFile = config;

			InventoryPriority = config.Bind<int>("Storage", "Inventory Priority", 1, "The priority of selecting from the player inventory. Lower numbers take precendence. Set to a negative number to disable");
			StoragePriority = config.Bind<int>("Storage", "Storage Priority", 2, "The priority of selecting from storage chests. Lower numbers take precendence. Set to a negative number to disable");
			StationsPriority = config.Bind<int>("Storage", "Stations Priority", 3, "The priority of selecting from stations. Lower numbers take precendence. Set to a negative number to disable");

			DepositDronesPLS = config.Bind<bool>("Planetary Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Planetary Logistics Station");
			DroneAmountPLS = config.Bind<int>("Planetary Logistics", "Planetary Drone Amount", -1, "How many drones should be added to a newly created Planetary Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

			DepositDronesILS = config.Bind<bool>("Interstellar Logistics", "Deposit Drones", true, "Whether drones should automatically be added to a newly created Interstellar Logistics Station");
			DroneAmountILS = config.Bind<int>("Interstellar Logistics", "Interstellar Drone Amount", -1, "How many drones should be added to a newly created Interstellar Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

			DepositVesselsILS = config.Bind<bool>("Interstellar Logistics", "Deposit Vessels", true, "Whether vessels should automatically be added to a newly created Interstellar Logistics Station");
			VesselAmountILS = config.Bind<int>("Interstellar Logistics", "Interstellar Vessel Amount", -1, "How many vessels should be added to a newly created Interstellar Logistics Station. Numbers less than 0 or greater than the max are treated as the normal drone maximum");

			DepositBots = config.Bind<bool>("Logistics Bots", "Deposit Bots", true, "Whether bots should automatically be added to a newly created logistics distrubution ports");
			BotAmount = config.Bind<int>("Logistics Bots", "Bots Amount", -1, "How many bots should be added to a newly created distribution port. Numbers less than 0 or greater than the max are treated as the normal bot maximum");
		}

		public static void ResetAllToDefault()
		{
			foreach (var configKey in ConfigFile.Keys)
			{
				var setting = ConfigFile[configKey];
				setting.BoxedValue = setting.DefaultValue;
			}
		}
	}
}
