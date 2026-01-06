using Adinmo.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreSelector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if ADINMO_UNITY_STORE_V4
        gameObject.AddComponent<StoreHandler>();
#elif ADINMO_UNITY_STORE_V5
        gameObject.AddComponent<StoreHandler>();
#else
        GameObject.Instantiate(Resources.Load("Fake StoreHandler"),this.transform);
#endif
    }


}
