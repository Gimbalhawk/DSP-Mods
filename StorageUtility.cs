using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DSP_Mods
{
	class StorageUtility
	{
		public bool UseInventory { get; set; }
		public bool UseStorage { get; set; }
		public bool UseStations { get; set; }

		public int RemoveItems(int itemId, int desiredAmount)
		{
			var player = GameMain.mainPlayer;
			var factory = player?.factory;

			if (player == null || factory == null)
				return 0;

			int removed = 0;

			if (UseInventory && desiredAmount > 0)
			{
				removed += RemoveFromPlayer(player, itemId, desiredAmount);
				desiredAmount -= removed;
			}

			if (UseStorage && desiredAmount > 0)
			{
				removed += RemoveFromStorage(factory, itemId, desiredAmount);
				desiredAmount -= removed;
			}

			if (UseStations && desiredAmount > 0)
			{
				removed += RemoveFromStations(factory, itemId, desiredAmount);
				desiredAmount -= removed;
			}

			return removed;
		}

		private int RemoveFromPlayer(Player player, int itemId, int desiredAmount)
		{
			if (player == null || player.package == null)
				return 0;

			var amountRemoved = player.package.TakeItem(itemId, desiredAmount);
			return amountRemoved;
		}

		private int RemoveFromStorage(PlanetFactory factory, int itemId, int desiredAmount)
		{
			int amountRemoved = 0;

			for (int i = 0; i < factory.factoryStorage.storageCursor && amountRemoved < desiredAmount; ++i)
			{
				var storage = factory.factoryStorage.storagePool[i];
				if (storage?.id != i)
					continue;

				amountRemoved += storage.TakeItem(itemId, desiredAmount - amountRemoved);
			}

			return amountRemoved;
		}

		private int RemoveFromStations(PlanetFactory factory, int itemId, int desiredAmount)
		{
			int amountRemoved = 0;

			for (int i = 0; i < factory.transport.stationCursor && amountRemoved < desiredAmount; ++i)
			{
				var station = factory.transport.stationPool[i];
				if (station?.id != i)
					continue;

				int idRef = itemId;
				int needed = desiredAmount - amountRemoved;
				station.TakeItem(ref idRef, ref needed);

				amountRemoved += needed;
			}

			return amountRemoved;
		}
	}
}
