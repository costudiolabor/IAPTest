using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class UIProduct : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI NameText;
   [SerializeField] private TextMeshProUGUI DescriptionText;
   [SerializeField] private Image Icon;
   [SerializeField] private TextMeshProUGUI PriceText;
   [SerializeField] private Button PuchaseButton;

   public delegate void PurchaseEvent(Product Model, Action onComplete);
   public event PurchaseEvent OnPurchase;
   
   [SerializeField] private Product Model;
   
   public void Setup(Product Product)
   {
      Model = Product;
      NameText.SetText(Product.metadata.localizedTitle);
      DescriptionText.SetText(Product.metadata.localizedDescription);
      PriceText.SetText($"{Product.metadata.localizedPriceString} " +
                        $"{Product.metadata.isoCurrencyCode}");
      Texture2D texture = StoreIconProvider.GetIcon(Product.definition.id);
      if (texture != null)
      {
          Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
              Vector2.one / 2f);
          Icon.sprite = sprite;
      }
      else
      {
          Debug.Log($"No Sprite found for {Product.definition.id}");
      }
   }

   public void Purchase()
   {
       PuchaseButton.enabled = false;
       OnPurchase?.Invoke(Model, HandlePurchaseComplete);
   }

   private void HandlePurchaseComplete()
   {
       PuchaseButton.enabled = true;
   }
}
