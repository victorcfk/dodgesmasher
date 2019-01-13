using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class MyIAPManager : MonoBehaviour {

    private IStoreController controller;
    private IExtensionProvider extensions;
    
    Action OnPurchaseFullGameSuccess;
    Action OnPurchaseShardsSuccess;

    Action OnPurchaseFullGameFailure;
    Action OnPurchaseShardsFailure;

    Action OnStoreInitializationSuccess;
    Action OnStoreInitializationFailure;

    public void Initialise(
        Action inPurchaseFullGameSuccessCallback,
        Action inPurchaseShardsSuccessCallback,
        Action inPurchaseFullGameFailureCallback,
        Action inPurchaseShardsFailureCallback,
        Action inOnStoreInitializationSuccess = null,
        Action inOnStoreInitializationFailure = null)
    {
        OnPurchaseFullGameSuccess += inPurchaseFullGameSuccessCallback;
        OnPurchaseShardsSuccess += inPurchaseShardsSuccessCallback;

        OnPurchaseFullGameFailure += inPurchaseFullGameFailureCallback;
        OnPurchaseShardsFailure += inPurchaseShardsFailureCallback;

        OnStoreInitializationFailure += inOnStoreInitializationFailure;

        //var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        
        //builder.AddProduct("fullgame", ProductType.NonConsumable);
        //builder.AddProduct("shardsbig", ProductType.Consumable);

        //UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        if (OnStoreInitializationSuccess != null) OnStoreInitializationSuccess();
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("Init fail" + error);
        if (OnStoreInitializationFailure != null) OnStoreInitializationFailure();
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log("We have a purchase:" + e.ToString());

        return PurchaseProcessingResult.Pending;
    }
    
    //Assume this is always called at start
    public void OnPurchaseSuccess(Product product)
    {
        if (product != null)
        {
            Debug.Log("purchase succeeded: " + product.definition.id);

            switch (product.definition.id)
            {
                case "fullgame":
                    OnPurchaseFullGameSuccess();

                    break;

                case "shardsbig":
                    OnPurchaseShardsSuccess();

                    break;
                default:
                    Debug.Log(
                    string.Format("Unrecognized productId \"{0}\"", product.definition.id)
                        );
                    break;
            }
        }
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason FailureReason)
    {
        
        if (product != null)
        {
            Debug.Log("purchase failed: " + product.definition.id + "-" + FailureReason);

            switch (product.definition.id)
            {
                case "fullgame":
                    OnPurchaseFullGameFailure();

                    break;

                case "shardsbig":
                    Debug.Log("shards!");
                    OnPurchaseShardsFailure();

                    break;
                default:
                    Debug.Log(
                    string.Format("Unrecognized productId \"{0}\"", product.definition.id)
                        );
                    break;
            }
        }
    }
}
