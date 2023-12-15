using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace DSP_Mods
{
	public class ModConfigWindow
	{
		private static Rect WindowRect = new Rect((Screen.width / 3) * 2, 100f, 500f, 420f);

		public static bool Visible { get; set; }

		public static void Open()
		{
			Visible = true;
		}

		public static void Close()
		{
			Visible = false;
		}

		public static void OnGUI()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Visible = false;
				return;
			}

			WindowRect = GUILayout.Window(1786512, WindowRect, WindowFunction, "AutoLogisticsDroneSetup config");
		}

		private static void WindowFunction(int id)
		{
			GUILayout.BeginArea(new Rect(WindowRect.width - 25f, 0f, 25f, 25f));
			if (GUILayout.Button("X"))
			{
				Close();
				return;
			}

			GUILayout.EndArea();

			GUILayout.BeginVertical(GUI.skin.box);

			DrawResetButton();

			string previousSection = "";
			foreach (var configDef in ModConfig.ConfigFile.Keys)
			{
				if (previousSection != configDef.Section)
				{
					DrawSectionLabel(configDef.Section);
				}

				DrawOption(ModConfig.ConfigFile[configDef]);

				previousSection = configDef.Section;
			}

			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private static void DrawResetButton()
		{
			GUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

			var clicked = GUILayout.Button("Restore Defaults", GUILayout.Width(125));
			if (clicked)
			{
				ModConfig.ResetAllToDefault();
			}

			GUILayout.EndVertical();
		}

		private static void DrawSectionLabel(string text)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(text);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private static void DrawOption(ConfigEntryBase configEntry)
		{
			GUILayout.BeginHorizontal();

			var textContent = new GUIContent(configEntry.Definition.Key, configEntry.Description.Description);
			GUILayout.Label(textContent);
			GUILayout.FlexibleSpace();

			if (configEntry.SettingType == typeof(bool))
			{
				DrawBoolValue(configEntry, 75);
			}
			else if (configEntry.SettingType == typeof(int))
			{
				DrawIntValue(configEntry, 75);
			}
			else
			{
				Logger.Instance.LogInfo($"Attempted to draw invalid option type for setting {configEntry.Definition.Key}. Setting is of type {configEntry.SettingType}");
			}

			GUILayout.EndHorizontal();
		}

		private static void DrawBoolValue(ConfigEntryBase configEntry, float width)
		{
			if (configEntry.SettingType != typeof(bool))
			{
				Logger.Instance.LogInfo($"Attempted to draw invalid ccnfig type. Expected bool but got {configEntry.SettingType}");
				return;
			}

			var value = (bool)configEntry.BoxedValue;

			GUILayout.BeginHorizontal(GUILayout.Width(width));

			var newValue = GUILayout.Toggle(value, value ? "Enabled" : "Disabled");
			if (newValue != value)
			{
				configEntry.BoxedValue = newValue;
			}

			GUILayout.EndHorizontal();
		}

		private static void DrawIntValue(ConfigEntryBase configEntry, float width)
		{
			if (configEntry.SettingType != typeof(int))
			{
				Logger.Instance.LogInfo($"Attempted to draw invalid ccnfig type. Expected int but got {configEntry.SettingType}");
				return;
			}

			var value = (int)configEntry.BoxedValue;

			GUILayout.BeginHorizontal(GUILayout.Width(width));

			var stringValue = GUILayout.TextField(value.ToString());
			if (Int32.TryParse(stringValue, out int newValue))
			{
				if (newValue != value)
				{
					configEntry.BoxedValue = newValue;
				}
			}

			GUILayout.EndHorizontal();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(UIGame), "On_E_Switch")]
		public static bool On_E_Switch_Prefix()
		{
			// Close our config menu if the user opens their inventory
			if (Visible)
			{
				Visible = false;
			}

			return true;
		}
	}
}
