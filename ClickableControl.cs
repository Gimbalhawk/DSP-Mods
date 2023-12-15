using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DSP_Mods
{
	internal class ClickableControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		private GameObject hoverTextObj;
		public string HoverText = "";
		public event Action<PointerEventData> OnClick;
		public Text TextObject;

		public void Start()
		{
			if (TextObject != null)
			{
				hoverTextObj = TextObject.gameObject;
				hoverTextObj.SetActive(false);
				TextObject.text = HoverText;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (hoverTextObj == null)
			{
				Start();
			}
			else
			{
				hoverTextObj.SetActive(true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (hoverTextObj == null)
			{
				Start();
			}
			else
			{
				hoverTextObj.SetActive(false);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			OnClick?.Invoke(eventData);
		}
	}
}
