
#if ADINMO_UNITY_STORE_V5

using UnityEngine;


using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;

using System.Collections.Generic;


using Unity.Services.Core;
using UnityEngine.Purchasing;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing.Extension;

 namespace Adinmo.Examples
{   /// <summary>
    /// A IAP store that uses the standard Unity In App Purchasing package from the Package Manager
    /// </summary>
    /// 

    public class StoreHandler : MonoBehaviour

    {
        AdinmoStoreController m_StoreController;

        protected void Awake()
        {
            InitializeIAP();
        }

        async void InitializeIAP()
        {
            m_StoreController = new AdinmoStoreController();

            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;

            await m_StoreController.Connect();

            FetchProducts();
        }

        void OnPurchasePending(PendingOrder order)
        {
            // Add your validations here before confirming the purchase.

            // Before confirming the purchase, reward the entitlement to the player.
            m_StoreController.ConfirmPurchase(order);
        }
        /// <summary>
        /// method for processing confirmed purchases
        /// </summary>
        /// <param name="order"></param>
        void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    // put in code for handling a failed order
                    LogConsole($"Purchase confirmation failed: {failedOrder.CartOrdered.Items().First().Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}");
                    break;
                case ConfirmedOrder:
                    // put in code for handling a successful order
                    LogConsole($"Purchase completed:  {order.CartOrdered.Items().First().Product.definition.id}");
                    break;
            }
        }

        void FetchProducts()
        {
            var initialProductsToFetch = new List<ProductDefinition>
            {
                //insert your products here
            };
            m_StoreController.FetchProducts(initialProductsToFetch);
        }
        /// <summary>
        /// code for handling purchasing an IAP
        /// </summary>
        /// <param name="productId"></param>
        public void InitiatePurchase(string productId)
        {
            var product = m_StoreController?.GetProducts().FirstOrDefault(product => product.definition.id == productId);

            if (product != null)
            {
                m_StoreController?.PurchaseProduct(product);
            }
            else
            {
                LogConsole($"The product service has no product with the ID {productId}");
            }
        }

        public void RestorePurchases()
        {
            m_StoreController.RestoreTransactions(OnTransactionsRestored);
        }

        void OnTransactionsRestored(bool success, string error)
        {
            LogConsole("Transactions restored: " + success);
        }

        void LogConsole(string msg)
        {
            Debug.Log(msg);
        }

    }
}
#endif