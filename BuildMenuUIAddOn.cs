using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSP_Mods
{
	public class BuildMenuUIAddOn : MonoBehaviour
	{
        private GameObject ConfigButton;
		private Image ConfigIconImage;

		public static BuildMenuUIAddOn InitUI(UIBuildMenu buildMenu)
		{
			GameObject buildMenuContainer = GameObject.Find("UI Root/Overlay Canvas/In Game/Function Panel/Build Menu/child-group");
			var containerRect = buildMenu.GetComponent<RectTransform>();

			if (containerRect == null)
				return null;

			Text textComponent = null;
			var firstButton = GameObject.Find("UI Root/Overlay Canvas/In Game/Function Panel/Build Menu/child-group/button-1");
			if (firstButton != null)
			{
				var count = firstButton.GetComponent<RectTransform>().transform.Find("count");
				textComponent = count?.GetComponentInChildren<Text>();
			}

			var ui = containerRect.gameObject.AddComponent<BuildMenuUIAddOn>();
			ui.AddModComponents(containerRect, textComponent);
			return ui;
		}

		public void AddModComponents(RectTransform containerRect, Text textComponent)
		{
			InitConfigButton(containerRect, textComponent);
		}

		public void Show()
		{
			ConfigIconImage.gameObject.SetActive(true);
		}

		public void Hide()
		{
			ConfigIconImage.gameObject.SetActive(false);
		}

		private void InitConfigButton(RectTransform containerRect, Text textComponent)
		{
			ConfigButton = new GameObject("ALDS_Config");
			var rect = ConfigButton.AddComponent<RectTransform>();
			rect.SetParent(containerRect.transform, false);

			rect.anchorMax = new Vector2(0, 0.5f);
			rect.anchorMin = new Vector2(0, 0.5f);
			rect.sizeDelta = new Vector2(20, 20);
			rect.pivot = new Vector2(0, 1);
			rect.anchoredPosition = new Vector2(210, 25);

			var invokeConfig = rect.gameObject.AddComponent<ClickableControl>();
			invokeConfig.HoverText = "Open AutoLogisticsDroneSetup config";

			if (textComponent != null)
			{
				var configHover = Instantiate(textComponent, containerRect.transform, true);
				var hoverRectTransform = configHover.GetComponent<RectTransform>();
				var parentRect = containerRect.GetComponent<RectTransform>();
				hoverRectTransform.anchorMax = new Vector2(0, 0.5f);
				hoverRectTransform.anchorMin = new Vector2(0, 0.5f);
				hoverRectTransform.sizeDelta = new Vector2(200, 25);
				hoverRectTransform.pivot = new Vector2(0.5f, 1);
				hoverRectTransform.anchoredPosition = new Vector2(225, 72);

				invokeConfig.TextObject = configHover;
			}

			ConfigIconImage = invokeConfig.gameObject.AddComponent<Image>();
			ConfigIconImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
			var configImgGameObject = GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/button-3-bg/button-3/icon");

			ConfigIconImage.sprite = configImgGameObject.GetComponent<Image>().sprite;
			invokeConfig.OnClick += data => { ModConfigWindow.Visible = !ModConfigWindow.Visible; };
		}
	}
}
