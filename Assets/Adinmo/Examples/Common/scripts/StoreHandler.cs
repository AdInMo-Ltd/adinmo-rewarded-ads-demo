//#define UNITY_STORE


using UnityEngine;


#if ADINMO_UNITY_STORE_V4
using Unity.Services.Core;
using UnityEngine.Purchasing;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing.Extension;

namespace Adinmo.Examples
{
    /// <summary>
    /// A IAP store that uses the standard Unity In App Purchasing package from the Package Manager
    /// </summary>
    /// 

    public class StoreHandler : MonoBehaviour, IDetailedStoreListener

    {

        private IStoreController StoreController;
        private IExtensionProvider ExtensionProvider;

        private async void Awake()
        {
            InitializationOptions options = new InitializationOptions()
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            .SetEnvironmentName("test");
#else
        .SetEnvironmentName("production");
#endif
            await UnityServices.InitializeAsync(options);
            ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
            operation.completed += HandleIAPCatalogLoaded;


        }

        void Start()
        {
        }

        private void HandleIAPCatalogLoaded(AsyncOperation Operation)
        {
            ResourceRequest request = Operation as ResourceRequest;
            Debug.Log($"Loaded Asset: {request.asset}");
            ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
            Debug.Log($"Loaded catalog with {catalog.allValidProducts.Count} items");
            //StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
            //StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#if UNITY_ANDROID
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.GooglePlay)
                );
#elif UNITY_IOS
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.AppleAppStore)
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
            UnityPurchasingAdInMoExtensions.InitializeWithAdInMo(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            StoreController = controller;
            ExtensionProvider = extensions;
        }



        public void PurchaseItem(string item)
        {
            if (StoreController != null)
            {
                foreach (var product in StoreController.products.all)
                {
                    if (product.definition.id == item)
                    {
                        Debug.Log("want to buy " + product.definition.id);
                        StoreController.InitiatePurchase(item);
                        return;
                    }
                }
                Debug.LogWarning("couldn't find iap " + item);
            }
            else
            {
                Debug.LogError("Can't make purchase, storecontroller is not initialised");
            }
        }


        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Error initializing IAP because of {error} {message}." +
                     $"\r\nShow a message to the player depending on this error");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log("outer handling purchase failed des");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log("outer handling purchase failed reas");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Debug.Log("outer handling processpurchase");
            return PurchaseProcessingResult.Complete;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Error initializing IAP because of {error}." +
                    $"\r\nShow a message to the player depending on this error");
        }
    }
}

#endif