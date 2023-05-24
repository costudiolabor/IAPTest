using System;
using UnityEngine;
using UnityEngine.Purchasing; //библиотека с покупками, будет доступна когда активируем сервисы

public class IAPCore : MonoBehaviour, IStoreListener //для получения сообщений из Unity Purchasing
{
    [SerializeField] private GameObject panelAds;
    [SerializeField] private GameObject panelVIP;

    [SerializeField] private GameObject panelAds_Done;
    [SerializeField] private GameObject panelVIP_Done;

    private static IStoreController m_StoreController;          //доступ к системе Unity Purchasing
    private static IExtensionProvider m_StoreExtensionProvider; // подсистемы закупок для конкретных магазинов

    public static string noads = "noads"; //одноразовые - nonconsumable
    public static string vip = "vip"; //одноразовые - nonconsumable или может быть подписка
    public static string coins151 = "coins151"; //многоразовые - consumable

    void Start()
    {
        if (m_StoreController == null) //если еще не инициализаровали систему Unity Purchasing, тогда инициализируем
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) //если уже подключены к системе - выходим из функции
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //Прописываем свои товары для добавления в билдер
        builder.AddProduct(noads, ProductType.NonConsumable);
        builder.AddProduct(vip, ProductType.NonConsumable); //или ProductType.Subscription
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

    void BuyProductID(string productId)
    {
        if (IsInitialized()) //если покупка инициализирована 
        {
            Debug.Log("buy");
            Product product = m_StoreController.products.WithID(productId); //находим продукт покупки 

            if (product != null && product.availableToPurchase) //если продукт найдет и готов для продажи
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product); //покупаем
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) //контроль покупок
    {
        if (String.Equals(args.purchasedProduct.definition.id, noads, StringComparison.Ordinal)) //тут заменяем наш ID
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //действия при покупке
            if (PlayerPrefs.HasKey("ads") == false)
            {
                PlayerPrefs.SetInt("ads", 0);
                panelAds.SetActive(false);
                panelAds_Done.SetActive(true);

              //  AdsCore.S.HideBanner();
             //   AdsCore.S.StopAllCoroutines();
            }

        }
        else if (String.Equals(args.purchasedProduct.definition.id, vip, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //действия при покупке
            if (PlayerPrefs.HasKey("vip") == false)
            {
                PlayerPrefs.SetInt("vip", 0);
                panelVIP.SetActive(false);
                panelVIP_Done.SetActive(true);
            }
        }
        else if (String.Equals(args.purchasedProduct.definition.id, coins151, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //действия при покупке
           // GameLogic.S.IncrementPoint2AfterAds(151);
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        return PurchaseProcessingResult.Complete;
    }

    public void RestorePurchases() //Восстановление покупок (только для Apple). У гугл это автоматический процесс.
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer) //если запущенно на эпл устройстве
        {
            Debug.Log("RestorePurchases started ...");

            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();

            apple.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("ads") == true)
        {
            panelAds.SetActive(false);
            panelAds_Done.SetActive(true);
        }
        else
        {
            panelAds.SetActive(true);
            panelAds_Done.SetActive(false);
        }

        if (PlayerPrefs.HasKey("vip") == true)
        {
            panelVIP.SetActive(false);
            panelVIP_Done.SetActive(true);
        }
        else
        {
            panelVIP.SetActive(true);
            panelVIP_Done.SetActive(false);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }



}
