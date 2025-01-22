using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS_Framework.ZuluWar
{
    public abstract class UI_BaseSellable : MonoBehaviour
    {
        [Header("=== UI_BaseSellable ===")]
        [SerializeField]
        protected TextMeshProUGUI titleText;
        [SerializeField]
        protected TextMeshProUGUI priceText;

        [Space(10)]
        [SerializeField]
        protected int _price;
        [SerializeField]
        protected int priceDelta;

        protected Button _button;
        protected UI_Minimap _minimap;

        public virtual void Initialize(UI_Minimap minimap)
        {
            _button = this.GetComponent<Button>();
            _button.onClick.AddListener(OnClickButton);

            this._minimap = minimap;

            UI_BaseSellable[] sellables = this.GetComponentsInChildren<UI_BaseSellable>(true);
            foreach (UI_BaseSellable sellable in sellables)
            {
                if (sellable != this)
                {
                    sellable.Initialize(_minimap);
                }
            }
        }

        public void Set(string title, int price)
        {
            titleText.text = title;
            priceText.text = price.ToString() + " Gold";

            this._price = price;
        }

        protected virtual void OnClickButton()
        {
            if (TryPurchase(OnPurchased) == false)
            {
                Debug.Log("Not enough money");
            }
        }

        private bool TryPurchase(Action onPurchased)
        {
            if (GameManager.Instance._PhaseCounter.TotalMoney >= _price)
            {
                GameManager.Instance._PhaseCounter.TotalMoney -= _price;
                onPurchased.Invoke();

                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void OnPurchased();
    }
}