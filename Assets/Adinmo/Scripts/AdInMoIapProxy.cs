// AdInMoIapProxy.cs
// Unity-style extension method for seamless AdInMo IAPBoost integration
// Usage: UnityPurchasing.InitializeWithAdInMo(this, builder);
# if ADINMO_UNITY_STORE_V4
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;


namespace Adinmo
{
    public delegate void OnPrePurchaseCallback(string productID, Product product);
    /// <summary>
    /// Extension methods for Unity Purchasing to enable AdInMo IAPBoost integration
    /// </summary>
    public static class UnityPurchasingAdInMoExtensions
    {
        /// <summary>
        /// Initialize Unity Purchasing with AdInMo IAPBoost support in a single call
        /// </summary>
        /// <param name="listener">Your IStoreListener implementation</param>
        /// <param name="builder">Unity IAP ConfigurationBuilder</param>
        public static void InitializeWithAdInMo(IStoreListener listener, ConfigurationBuilder builder, OnPrePurchaseCallback onPrePurchase = null)
        {
            AdinmoUtilities.LogInfo("Initializing Unity Purchasing with AdInMo IAPBoost support...");
            AdInMoIapProxy.Register(onPrePurchase);

            // Use newer IDetailedStoreListener API to avoid deprecation warnings
            var wrappedListener = AdInMoIapProxy.Wrap(listener);
            if (wrappedListener is IDetailedStoreListener detailedListener)
            {
                UnityPurchasing.Initialize(detailedListener, builder);
            }
            else
            {
                // Fallback for compatibility
#pragma warning disable CS0618
                UnityPurchasing.Initialize(wrappedListener, builder);
#pragma warning restore CS0618
            }
        }


    }

    /// <summary>
    /// Internal proxy that handles AdInMo IAPBoost callbacks and reporting
    /// </summary>
    internal static class AdInMoIapProxy
    {
        /// <summary>
        /// Event fired before initiating a purchase. Subscribe to run custom logic.
        /// </summary>
        public static OnPrePurchaseCallback OnPrePurchase;

        /// <summary>
        /// Wraps an IStoreListener with AdInMo reporting capabilities
        /// </summary>
        public static IStoreListener Wrap(IStoreListener inner) => new Proxy(inner);

        /// <summary>
        /// Registers all AdInMo IAPBoost callbacks
        /// </summary>
        public static void Register(OnPrePurchaseCallback onPrePurchase)
        {
            OnPrePurchase = onPrePurchase;

            if (_registered) return;
            _registered = true;

            AdinmoUtilities.LogInfo("Registering AdInMo IAPBoost callbacks...");

            // 1) When an AdInMo UI/ad requests a purchase, start Unity IAP for that SKU
            AdinmoManager.SetInAppPurchaseCallback(OnAdInMoPurchaseRequested);

            // 2) Let AdInMo show localized store prices in their magnifier CTA
            AdinmoManager.SetInAppPurchaseGetPriceCallback(GetLocalizedPriceString);

            // 3) Let AdInMo know if a non-consumable is already owned (for gating UI/serving)
            // Use boolean callback for maximum compatibility across AdInMo SDK versions
            AdinmoManager.SetInAppPurchasedAlreadyCallback(iapId =>
            {
                // Fallback: try to get controller from Unity IAP if proxy wasn't initialized properly
                if (_controller == null)
                {
                    TryGetControllerFromUnityIAP();
                }

                var product = _controller?.products?.WithID(iapId);
                bool isPurchased = product != null && product.hasReceipt;

                AdinmoUtilities.LogInfo($"Already purchased check for {iapId}: {isPurchased}");
                if (isPurchased)
                    return new InAppAlreadyPurchasedReply(true, (float)product.metadata.localizedPrice, product.metadata.isoCurrencyCode);
                return new InAppAlreadyPurchasedReply(false);
            });

            AdinmoUtilities.LogInfo("All callbacks registered successfully!");
        }

        /// <summary>
        /// Validates that all requested product IDs are properly configured in Unity IAP
        /// Call this after IAP initialization for debugging purposes
        /// </summary>
        public static void ValidateProductConfiguration()
        {
            if (_controller?.products?.all == null)
            {
                AdinmoUtilities.LogWarning("Cannot validate products - IAP not initialized", true);
                return;
            }

            var registeredProducts = _controller.products.all.Select(p => p.definition.id).ToArray();
            AdinmoUtilities.LogInfo($"Registered Unity IAP Products: [{string.Join(", ", registeredProducts)}]");
            AdinmoUtilities.LogInfo(" Ensure all AdInMo campaign SKU IDs match these product IDs to avoid purchase failures");

#if UNITY_EDITOR
            AdinmoUtilities.LogInfo("💡 TIP: Check your AdInMo dashboard campaigns and verify all SKU IDs are listed above");
#endif
        }

        // ---- internal state ----
        static bool _registered;
        static IStoreController _controller;

        static System.Collections.Generic.Dictionary<string, Product> _pendingPurchases = new System.Collections.Generic.Dictionary<string, Product>();

        /// <summary>
        /// Continue a paused purchase. Only needed when PauseBeforePurchase = true.
        /// </summary>
        public static void ContinuePurchase(string productId)
        {
            if (!_pendingPurchases.TryGetValue(productId, out var product))
            {
                AdinmoUtilities.LogWarning($"No pending purchase found for: {productId}");
                return;
            }

            _pendingPurchases.Remove(productId);
            AdinmoUtilities.LogInfo($"Continuing paused purchase for: {productId}");
            _controller.InitiatePurchase(product);
        }

        /// <summary>
        /// Fallback method to get the controller when proxy initialization was bypassed
        /// </summary>
        static void TryGetControllerFromUnityIAP()
        {
            try
            {
                // Try to get the controller from any active StoreHandler instances
                var storeHandlers = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var handler in storeHandlers)
                {
                    // Look for StoreController field/property in active components
                    var storeControllerField = handler.GetType().GetField("StoreController",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (storeControllerField != null)
                    {
                        var controller = storeControllerField.GetValue(handler) as IStoreController;
                        if (controller != null)
                        {
                            _controller = controller;
                            AdinmoUtilities.LogInfo("Found and set controller from StoreHandler fallback");
                            return;
                        }
                    }
                }

                AdinmoUtilities.LogWarning("Could not find active StoreController via fallback method", true);
            }
            catch (Exception ex)
            {
                AdinmoUtilities.LogError($"Error in fallback controller lookup: {ex.Message}", true);
            }
        }

        // ---- AdInMo callbacks ----

        internal static void OnAdInMoPurchaseRequested(string iapId)
        {
            AdinmoUtilities.LogInfo($"Purchase requested from AdInMo for: {iapId}");

            // Fallback: try to get controller from Unity IAP if proxy wasn't initialized properly
            if (_controller == null)
            {
                TryGetControllerFromUnityIAP();
            }

            if (_controller == null)
            {
                AdinmoUtilities.LogError("Cannot process purchase - IAP not initialized yet",true);
                ReportPurchaseUnavailable(iapId, "IAP system not initialized");
                return;
            }

            var product = _controller.products?.WithID(iapId);
            if (product == null)
            {
                AdinmoUtilities.LogError($"Product '{iapId}' not registered in Unity IAP configuration",true);
                ReportPurchaseUnavailable(iapId, "Product not configured in Unity IAP");
                return;
            }

            if (!product.availableToPurchase)
            {
                AdinmoUtilities.LogWarning($"Product '{iapId}' is not available for purchase");
                ReportPurchaseUnavailable(iapId, "Product not available for purchase", product);
                return;
            }

            // If pause mode enabled, store and wait for ContinuePurchase()
            if (OnPrePurchase != null)
            {
                try
                {
                    _pendingPurchases[iapId] = product;
                    AdinmoUtilities.LogInfo($"Purchase paused for: {iapId}. Call AdInMoIapProxy.ContinuePurchase(\"{iapId}\") to proceed.");
                    OnPrePurchase(iapId, product);
                }
                catch (Exception ex)
                {
                    AdinmoUtilities.LogError($"Error in OnPrePurchase handler: {ex.Message}");
                }

                return;
            }
            AdinmoUtilities.LogInfo($"Starting purchase for: {iapId}");
            _controller.InitiatePurchase(product);
        }

        /// <summary>
        /// Reports to AdInMo that a purchase cannot be completed due to configuration issues
        /// </summary>
        static void ReportPurchaseUnavailable(string iapId, string reason, Product product = null)
        {
            // Extract product metadata if available
            string currency = product?.metadata?.isoCurrencyCode ?? "USD";
            float price = product != null ? (float)(product.metadata?.localizedPrice ?? 0m) : 0f;

            AdinmoUtilities.LogError($"Reporting purchase failure to AdInMo: {iapId} - {reason}", true);

            try
            {
                // Report the configuration failure to AdInMo
                AdinmoManager.InAppPurchaseFailed(iapId, currency, price, reason);
            }
            catch (Exception ex)
            {
                AdinmoUtilities.LogError($"Failed to report purchase unavailability to AdInMo: {ex.Message}", true);
            }
        }

        static string GetLocalizedPriceString(string itemId)
        {
            if (_controller == null)
            {
                TryGetControllerFromUnityIAP();
            }

            var p = _controller?.products?.WithID(itemId);
            var price = p?.metadata?.localizedPriceString;
            AdinmoUtilities.LogInfo($"Price requested for {itemId}: {price}");
            return price;
        }

        // ---- Proxy wrapper for Unity IAP ----

        class Proxy : IDetailedStoreListener
        {
            readonly IStoreListener _inner;
            public Proxy(IStoreListener inner) { _inner = inner; }

            public void OnInitialized(IStoreController c, IExtensionProvider e)
            {
                _controller = c; // allow callbacks to query products & initiate purchases
                AdinmoUtilities.LogInfo("Unity IAP initialized, store controller ready", true);
                _inner.OnInitialized(c, e);
            }

            public void OnInitializeFailed(InitializationFailureReason error)
            {
                AdinmoUtilities.LogError($"Unity IAP initialization failed: {error}", true);
#pragma warning disable CS0612
                _inner.OnInitializeFailed(error);
#pragma warning restore CS0612
            }

            public void OnInitializeFailed(InitializationFailureReason error, string message)
            {
                AdinmoUtilities.LogError($"Unity IAP initialization failed: {error} - {message}", true);
                _inner.OnInitializeFailed(error, message);
            }

            public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
            {
                // Let game code decide success (Complete) vs Pending
                var result = _inner.ProcessPurchase(args);

                if (result == PurchaseProcessingResult.Complete)
                {
                    ReportSuccess(args.purchasedProduct);
                }
                return result;
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureDescription failure)
            {
                // Try calling the newer method first, fallback to older method
                try
                {
                    ((IDetailedStoreListener)_inner)?.OnPurchaseFailed(product, failure);
                }
                catch
                {
                    // Fallback to legacy method if newer interface not implemented
#pragma warning disable CS0618
                    _inner.OnPurchaseFailed(product, failure?.reason ?? PurchaseFailureReason.Unknown);
#pragma warning restore CS0618
                }
                ReportFailure(product, failure?.reason.ToString());
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
            {
#pragma warning disable CS0618
                _inner.OnPurchaseFailed(product, failureReason);
#pragma warning restore CS0618
                ReportFailure(product, failureReason.ToString());
            }

            // ---- Reporting helpers ----

            static void ReportSuccess(Product p)
            {
                if (p == null) return;
                var id = p.definition?.id ?? "";
                var curr = p.metadata?.isoCurrencyCode ?? "";
                var amt = (float)(p.metadata?.localizedPrice ?? 0m);
                var tx = p.transactionID;

                AdinmoUtilities.LogInfo($"Reporting success to AdInMo: {id}, {curr}, {amt}, {tx}");

                AdinmoManager.InAppPurchaseSuccess(id, curr, amt, tx);
            }

            static void ReportFailure(Product p, string reason)
            {
                if (p == null) return;
                var id = p.definition?.id ?? "";
                var curr = p.metadata?.isoCurrencyCode ?? "";
                var amt = (float)(p.metadata?.localizedPrice ?? 0m);

                AdinmoUtilities.LogInfo($"Reporting failure to AdInMo: {id}, reason: {reason}");

                // Newer SDKs accept a failure reason parameter.
                try
                {
                    AdinmoManager.InAppPurchaseFailed(id, curr, amt, reason);
                }
                catch
                {
                    // Older SDKs without reason/source overload:
                    AdinmoManager.InAppPurchaseFailed(id, curr, amt, null);
                }
            }
        }
    }
}
#endif
