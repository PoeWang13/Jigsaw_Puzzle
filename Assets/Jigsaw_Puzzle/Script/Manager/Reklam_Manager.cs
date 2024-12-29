using System;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System.Collections.Generic;

public class Reklam_Manager : Singletion<Reklam_Manager>
{
    // Reklam kimliği : ca-app-pub-1398478089736122~7472451513
    // InterRewarded : ca-app-pub-1398478089736122/4846288178
    // Rewarded : ca-app-pub-1398478089736122/2574082702
    [SerializeField] private bool isTest;
    [SerializeField] private bool useDebug;
    [SerializeField] private List<Button> reklamButtons = new List<Button>();

    private const string _adInterAdsId = "ca-app-pub-3940256099942544/5354046379";
    private const string _adRewardAdsId = "ca-app-pub-3940256099942544/5224354917";

    private const string _adTestInterAdsId = "ca-app-pub-3940256099942544/5354046379";
    private const string _adTestRewardAdsId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd _rewardedAd;
    private RewardedInterstitialAd _rewardedInterstitialAd;

    private float interLoadingTime;
    private float rewardedLoadingTime;

    private int reklamAmount;

    public void ShowReklam(Action action, bool isVideo)
    {
        if (isVideo)
        {
            RewardShowAd(action);
        }
        else
        {
            reklamAmount++;
            if (reklamAmount >= 5)
            {
                InterShowAd(action);
                reklamAmount = 0;
            }
            else
            {
                action?.Invoke();
            }
        }
    }
    public void ShowReklam(Action action)
    {
        RewardShowAd(action);
    }
    private void Start()
    {
        InterLoadAd();
        RewardLoadAd();
    }
    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void InterLoadAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedInterstitialAd != null)
        {
            InterDestroyAd();
        }
        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedInterstitialAd.Load(isTest ? _adTestInterAdsId : _adInterAdsId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    if (useDebug)
                    {
                        Debug.LogError("Rewarded interstitial ad failed to load an ad with error : " + error);
                    }
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpexted error, please report this bug if it happens.
                if (ad == null)
                {
                    if (useDebug)
                    {
                        Debug.LogError("Unexpected error: Rewarded interstitial load event fired with null ad and null error.");
                    }
                    return;
                }

                // The operation completed successfully.
                if (useDebug)
                {
                    Debug.LogWarning("Inter Reklam yüklendi.");
                }
                //Debug.LogWarning("Inter Reklam yüklendi." + ad.GetResponseInfo());
                _rewardedInterstitialAd = ad;

                // Register to ad events to extend functionality.
                InterRegisterEventHandlers(ad);
            });
    }
    /// <summary>
    /// Shows the ad.
    /// </summary>
    [ContextMenu("Show Inter Reklam")]
    public void InterShowAd(Action action)
    {
        if (_rewardedInterstitialAd == null)
        {
            // Reklamın yüklenmesi çok uzun sürmüş, silip tekrar dene.
            InterLoadAd();
        }
        else
        {
            if (_rewardedInterstitialAd.CanShowAd())
            {
                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    InterLoadAd();
                    action?.Invoke();
                    interLoadingTime = Time.time;
                    if (useDebug)
                    {
                        Debug.LogWarning("Inter Reklam gösterildi." + reward.Amount);
                    }
                });
            }
            else
            {
                if (Time.time - interLoadingTime > 10)
                {
                    // Reklamın yüklenmesi çok uzun sürmüş, silip tekrar dene.
                    InterLoadAd();
                }
                if (useDebug)
                {
                    Debug.LogError("Rewarded interstitial ad is not ready yet.");
                }
            }
        }
    }
    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void InterDestroyAd()
    {
        if (_rewardedInterstitialAd != null)
        {
            if (useDebug)
            {
                Debug.Log("Destroying rewarded interstitial ad.");
            }
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }
    }
    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void InterLogResponseInfo()
    {
        if (_rewardedInterstitialAd != null)
        {
            var responseInfo = _rewardedInterstitialAd.GetResponseInfo();

            if (useDebug)
            {
                Debug.Log(responseInfo);
            }
        }
    }
    protected void InterRegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            if (useDebug)
            {
                Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded interstitial ad recorded an impression.");
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded interstitial ad was clicked.");
            }
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded interstitial ad full screen content opened.");
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded interstitial ad full screen content closed.");
            }
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            if (useDebug)
            {
                Debug.LogError("Rewarded interstitial ad failed to open full screen content with error : " + error);
            }
        };
    }
    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void RewardLoadAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            RewardDestroyAd();
        }
        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(isTest ? _adTestRewardAdsId : _adRewardAdsId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                if (useDebug)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                }
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                if (useDebug)
                {
                    Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                }
                return;
            }

            // The operation completed successfully.
            if (useDebug)
            {
                Debug.LogWarning("Rewarded reklam yüklendi.");
            }
            _rewardedAd = ad;
            for (int e = 0; e < reklamButtons.Count; e++)
            {
                reklamButtons[e].interactable = true;
            }

            // Register to ad events to extend functionality.
            RewardRegisterEventHandlers(ad);
        });
    }
    /// <summary>
    /// Shows the ad.
    /// </summary>
    [ContextMenu("Show Reward Reklam")]
    public void RewardShowAd(Action action)
    {
        if (_rewardedAd == null)
        {
            if (useDebug)
            {
                Debug.LogError("Rewarded ad is not ready yet.");
            }
            if (Time.time - rewardedLoadingTime > 10)
            {
                RewardLoadAd();
            }
        }
        else
        {
            if (_rewardedAd.CanShowAd())
            {
                if (useDebug)
                {
                    Debug.Log("Rewarded reklam gösterilmeye başlandı.");
                }
                for (int e = 0; e < reklamButtons.Count; e++)
                {
                    reklamButtons[e].interactable = false;
                }
                _rewardedAd.Show((Reward reward) =>
                {
                    action?.Invoke();
                    if (useDebug)
                    {
                        Debug.Log("Rewarded reklam gösterildi.");
                    }
                    rewardedLoadingTime = Time.time;
                    //Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}", reward.Amount, reward.Type));
                    RewardLoadAd();
                });
            }
            else
            {
                if (useDebug)
                {
                    Debug.LogError("Rewarded ad is not ready yet.");
                }
                if (Time.time - rewardedLoadingTime > 10)
                {
                    RewardLoadAd();
                }
            }
        }
    }
    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void RewardDestroyAd()
    {
        if (_rewardedAd != null)
        {
            if (useDebug)
            {
                Debug.Log("Destroying rewarded ad.");
            }
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }
    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void RewardLogResponseInfo()
    {
        if (_rewardedAd != null)
        {
            var responseInfo = _rewardedAd.GetResponseInfo();
            if (useDebug)
            {
                Debug.Log(responseInfo);
            }
        }
    }
    private void RewardRegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            if (useDebug)
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded ad recorded an impression.");
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded ad was clicked.");
            }
        };
        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded ad full screen content opened.");
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            if (useDebug)
            {
                Debug.Log("Rewarded ad full screen content closed.");
            }
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            if (useDebug)
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);
            }
        };
    }
}