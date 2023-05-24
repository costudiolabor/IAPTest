using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class DemoStorePage : MonoBehaviour, IStoreListener
{
    [SerializeField] private UIProduct UIProductPrefab;
    [SerializeField] private HorizontalLayoutGroup ContentPanel;
    [SerializeField] private GameObject LoadingOverlay;

    private Action OnPurchaseCompleted;
    private static IStoreController StoreController;          // доступ к системе Unity Purchasing
    private static IExtensionProvider ExtensionProvider;     // подсистема закупок для конкр магазинов

    private async void Awake()
    {
        InitializationOptions options = new InitializationOptions()  // параметры инициализации
#if UNITY_EDITOR || development_build
            .SetEnvironmentName("test");  
#else
            .SetEnvironmentName("production");
#endif
        await UnityServices.InitializeAsync(options);
        ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog"); // загружаем асинхронно продукты
        operation.completed += HandleIAPCatalogLoaded;
    }

    private void HandleIAPCatalogLoaded(AsyncOperation Operation)
    {
        ResourceRequest request = Operation as ResourceRequest;
        
        Debug.Log($"Loaded Asset: {request.asset}");
        ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
        Debug.Log($"Loaded catalog with {catalog.allProducts.Count} items");

        StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        StandardPurchasingModule.Instance().useFakeStoreAlways = true;
        
        // конструктор конфигураций
#if UNITY_ANDROID
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
             StandardPurchasingModule.Instance(AppStore.GooglePlay)
        );
#elif UNITY_IOS
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
             StandardPurchasingModule.Instance(AppStore.AppleAppSrore)
        );
#else
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.NotSpecified)
        );
#endif
        foreach (ProductCatalogItem item in catalog.allProducts)
        {
            builder.AddProduct(item.id, item.type);
        }
        
        UnityPurchasing.Initialize(this, builder); // инициализируем процесс покупки
    }

    public void OnInitializeFailed(InitializationFailureReason error)    //если процесс инициализации терпит неудачу
    {
        Debug.Log($"Error initializing IAP because of {error}." +
                  $"\r\nShow a message to the player depending on the error.");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) //если процесс инициализации терпит неудачу
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) // если процесс успешен и игрок приобрел покупку, сняли плату и
                                                                                     // теперь нуно отдать предмет
    {
        Debug.Log($"Successfully purchased {purchaseEvent.purchasedProduct.definition.id}");
        OnPurchaseCompleted?.Invoke();
        OnPurchaseCompleted = null;
        LoadingOverlay.SetActive(false);

        return PurchaseProcessingResult.Complete;

    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)  //если процесс покупки терпит неудачу
    {
        Debug.Log($"FAILED to purchase {product.definition.id} because {failureReason}");
        OnPurchaseCompleted?.Invoke();
        OnPurchaseCompleted = null;
        LoadingOverlay.SetActive(false);

    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)  // если успешно инициализировался
    {
        StoreController = controller;
        ExtensionProvider = extensions;
        StoreIconProvider.Initialize(StoreController.products);
        StoreIconProvider.OnLoadComplete += HandleAllIconsLoaded;
    }

    private void HandleAllIconsLoaded()
    {
        StartCoroutine(CreateUI());
    }

    private IEnumerator CreateUI()
    {
        List<Product> sortedProducts = StoreController.products.all
            .OrderBy(item => item.metadata.localizedPrice)              // Сортируем по цене
            .ToList();
        foreach (Product product in sortedProducts)
        {
            UIProduct uiProduct = Instantiate(UIProductPrefab);
            uiProduct.OnPurchase += HandlePurchase;
            uiProduct.Setup(product);
            uiProduct.transform.SetParent(ContentPanel.transform, false);
            yield return null;
        }
    }

    private void HandlePurchase(Product Product, Action OnPurchaseCompleted)
    {
        LoadingOverlay.SetActive(true);
        this.OnPurchaseCompleted = OnPurchaseCompleted;
        StoreController.InitiatePurchase(Product);
    }
}
