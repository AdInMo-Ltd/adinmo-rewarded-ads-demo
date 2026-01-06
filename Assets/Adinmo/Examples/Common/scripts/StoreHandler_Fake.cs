



using UnityEngine;
using UnityEngine.UI;

namespace Adinmo
{ 
/// <summary>
/// a fake store used to demonstrate how to test basic store actions
/// </summary>
class FakeStoreHandler : MonoBehaviour
    {
        public GameObject StoreCanvas;
        public Text purchaseButton;
        static string iapID;
#if !ADINMO_UNITY_STORE_V4 && !ADINMO_UNITY_STORE_V5
        private void Start()
        {
            StoreCanvas=GetComponentInChildren<Canvas>(true).gameObject;
            var textObjects=GetComponentsInChildren<Text>(true);
            foreach (var textObject in textObjects)
            {
                if(textObject.name=="btnPurchase")
                {
                    purchaseButton = textObject;
                    break;
                }
            }
            AdinmoManager.SetInAppPurchaseCallback(PurchaseItem);
            AdinmoManager.SetInAppPurchaseGetPriceCallback(GetPrice);
            AdinmoManager.SetInAppPurchasedAlreadyCallback(CheckAlreadyPurchased);
        }
        public void PurchaseItem(string iap_id)
        {
            iapID = iap_id;
            StoreCanvas.SetActive(true);
            purchaseButton.text = "Purchase " + iapID + "?";
        }

        public void makePurchase()
        {
            AdinmoManager.InAppPurchaseSuccess(iapID, "GBP", 0.00f, "testTransaction");
            StoreCanvas.SetActive(false);
        }

        public void refusePurchase()
        {
            AdinmoManager.InAppPurchaseFailed(iapID, "GBP", 0.00f, "Declined purchase");
            StoreCanvas.SetActive(false);
        }

        public static string GetPrice(string iap_id)
        {
            return "$1.99";
        }

        public static InAppAlreadyPurchasedReply CheckAlreadyPurchased(string iap_id)
        {
            return new InAppAlreadyPurchasedReply(false);
        }
        
#endif
    }
}


