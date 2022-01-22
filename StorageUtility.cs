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

		private List<StorageSource> Sources { get; set; }

		public void AddSource(Source source, int priority)
		{
			if (priority < 0)
				return;

			if (Sources == null)
				Sources = new List<StorageSource>();

			Sources.Add(new StorageSource()
			{
				Source = source,
				Priority = priority
			});

			Sources = Sources.OrderBy(s => s.Priority).ToList();
		}

		public int RemoveItems(int itemId, int desiredAmount)
		{
			var player = GameMain.mainPlayer;
			var factory = player?.factory;

			if (player == null || factory == null)
				return 0;

			if (desiredAmount <= 0)
				return 0;

			int totalRemoved = 0;

			// The list of sources is sorted by priority, so this will pull from them in the order the player requested
			foreach (var s in Sources)
			{
				int removed = 0;

				switch (s.Source)
				{
					case Source.Inventory:
						removed = RemoveFromPlayer(player, itemId, desiredAmount);
						break;

					case Source.Storage:
						removed = RemoveFromStorage(factory, itemId, desiredAmount);
						break;

					case Source.Stations:
						removed = RemoveFromStations(factory, itemId, desiredAmount);
						break;
				}

				totalRemoved += removed;
				desiredAmount -= removed;

				if (desiredAmount <= 0)
					break;
			}

			return totalRemoved;
		}

		private int RemoveFromPlayer(Player player, int itemId, int desiredAmount)
		{
			if (player == null || player.package == null)
				return 0;

			int inc = 0;
			var amountRemoved = player.package.TakeItem(itemId, desiredAmount, out inc);
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

				int inc = 0;
				amountRemoved += storage.TakeItem(itemId, desiredAmount - amountRemoved, out inc);
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
				int inc = 0;
				station.TakeItem(ref idRef, ref needed, out inc);

				amountRemoved += needed;
			}

			return amountRemoved;
		}
	}
}
