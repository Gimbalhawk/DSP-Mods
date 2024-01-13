using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;

namespace DSP_Mods
{
	public struct Item
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	class StorageUtility
	{
		private PlayerPackageUtility PlayerPackageUtility = null;

		public enum Source
		{
			Inventory,
			Storage,
			Stations
		}

		private struct StorageSource
		{
			public Source Source { get; set; }
			public int Priority { get; set; }
		}

		// Get the list of valid storage sources sorted by priority
		private IEnumerable<StorageSource> GetStorageSources()
		{
			var sources = new List<StorageSource>();

			AddSource(sources, StorageUtility.Source.Inventory, ModConfig.InventoryPriority.Value);
			AddSource(sources, StorageUtility.Source.Storage, ModConfig.StoragePriority.Value);
			AddSource(sources, StorageUtility.Source.Stations, ModConfig.StationsPriority.Value);

			return sources.OrderBy(s => s.Priority).ToList();
		}

		private void AddSource(List<StorageSource> sources, Source source, int priority)
		{
			if (priority < 0)
				return;

			sources.Add(new StorageSource()
			{
				Source = source,
				Priority = priority
			});
		}

		public int RemoveItems(Item item, int desiredAmount)
		{
			var player = GameMain.mainPlayer;
			var factory = player?.factory;

			if (player == null || factory == null)
				return 0;

			if (desiredAmount <= 0)
				return 0;

			int totalRemoved = 0;

			var sources = GetStorageSources();
			// The list of sources is sorted by priority, so this will pull from them in the order the player requested
			foreach (var s in sources)
			{
				int removed = 0;

				switch (s.Source)
				{
					case Source.Inventory:
						removed = RemoveFromPlayer(player, item, desiredAmount);
						break;

					case Source.Storage:
						removed = RemoveFromStorage(factory, item, desiredAmount);
						break;

					case Source.Stations:
						removed = RemoveFromStations(factory, item, desiredAmount);
						break;
				}

				totalRemoved += removed;
				desiredAmount -= removed;

				if (desiredAmount <= 0)
					break;
			}

			return totalRemoved;
		}

		private int RemoveFromPlayer(Player player, Item item, int desiredAmount)
		{
			if (player == null || player.package == null)
				return 0;

			if (PlayerPackageUtility == null || PlayerPackageUtility.player != player)
				PlayerPackageUtility = new PlayerPackageUtility(player);

			int id = item.Id;
			int inc = 0;
			int amountRemoved = desiredAmount;
			PlayerPackageUtility.TryTakeItemFromAllPackages(ref id, ref amountRemoved, out inc, true);

			if (amountRemoved > 0)
				Logger.Instance.LogInfo($"Removed {amountRemoved} {item.Name}{(amountRemoved == 1 ? "" : "s")} from player");

			return amountRemoved;
		}

		private int RemoveFromStorage(PlanetFactory factory, Item item, int desiredAmount)
		{
			int amountRemoved = 0;

			for (int i = 0; i < factory.factoryStorage.storageCursor && amountRemoved < desiredAmount; ++i)
			{
				var storage = factory.factoryStorage.storagePool[i];
				if (storage?.id != i)
					continue;

				int id = item.Id;
				int inc = 0;
				int count = desiredAmount - amountRemoved;
				storage.TakeTailItems(ref id, ref count, out inc);
				amountRemoved += count;

				if (count > 0)
					Logger.Instance.LogInfo($"Removed {count} {item.Name}{(count == 1 ? "" : "s")} from storage");
			}

			return amountRemoved;
		}

		private int RemoveFromStations(PlanetFactory factory, Item item, int desiredAmount)
		{
			int amountRemoved = 0;

			for (int i = 0; i < factory.transport.stationCursor && amountRemoved < desiredAmount; ++i)
			{
				var station = factory.transport.stationPool[i];
				if (station?.id != i)
					continue;

				int idRef = item.Id;
				int needed = desiredAmount - amountRemoved;
				int inc = 0;
				station.TakeItem(ref idRef, ref needed, out inc);

				amountRemoved += needed;

				if (needed > 0)
					Logger.Instance.LogInfo($"Removed {needed} {item.Name}{(needed == 1 ? "" : "s")} from station");
			}

			return amountRemoved;
		}
	}
}
