using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing; // библ-ка с покупками 
public class IAPCoreMy : MonoBehaviour, IStoreListener // для получения сооб из Unity Purchasing
{
    [SerializeField] private GameObject panelAds;
    [SerializeField] private GameObject panelVIP;
    [SerializeField] private GameObject panelAdsDone;
    [SerializeField] private GameObject panelVIPDone;

    private static IStoreController storeController;          // доступ к системе Unity Purchasing
    private static IExtensionProvider storeExtensionProvider; // подсистема закупок для конкр магазинов

    public static string noads = "noads";        // одноразовые покуп
    public static string vip = "vip";            // одноразовые покуп 
    public static string coins151 = "coins151";  // многоразовые покуп


    private void Start() {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    private void InitializePurchasing()
    {
        //if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //добав свои товары
        builder.AddProduct(noads, ProductType.NonConsumable);
        builder.AddProduct(vip, ProductType.NonConsumable);
        builder.AddProduct(coins151, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void Buy_noads()
    {
        BuyProductID(noads);
    }

    public void Buy_vip()
    {
        BuyProductID(vip);
    }
    
    public void Buy_coins151()
    {
        BuyProductID(coins151);
    }
    
    private void BuyProductID(string productId) {

        //if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);      //находим продукт покупки
            if(product != null && product.availableToPurchase)                 //если продукт найден и готов к продаже
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                storeController.InitiatePurchase(product);                     //покупаем
            }
            else
            {
              Debug.Log("BuyProductID: FAIL. Not purchasing product");   
            }
        }
       // else
        {
              Debug.Log("BuyProductID: FAIL. Not initialized");   
        }
    }

  //  public PurchaseProcessingResult ProcessPurchase()
  //  {
        
  //  }
    
    

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        throw new System.NotImplementedException();
    }
}
