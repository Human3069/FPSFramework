using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS_Framework.ZuluWar
{
    public abstract class UI_BaseSellable : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI titleText;
        [SerializeField]
        protected TextMeshProUGUI priceText;

        protected Button _button;
        protected UI_Minimap _minimap;

        public virtual void Initialize(UI_Minimap minimap)
        {
            _button = this.GetComponent<Button>();
            _button.onClick.AddListener(OnClickButton);

            this._minimap = minimap;
        }

        protected abstract void OnClickButton();

        public void Set(string title, int price)
        {
            titleText.text = title;
            priceText.text = price.ToString() + " Gold";
        }
    }
}