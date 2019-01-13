using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class GooglePlayGamesManager : MonoBehaviour {
    
	// Use this for initialization
	public void Initialise () {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
         .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
    }

    public void CallForAuthenticate(System.Action OnSocialAuthenticateSuccess = null, System.Action OnSocialAuthenticateFailure = null)
    {
        Social.localUser.Authenticate((bool success) =>
        {
            // handle success or failure
            if (success)
            {
                Debug.Log("login successful");
                if(OnSocialAuthenticateSuccess != null)
                    OnSocialAuthenticateSuccess();
            }
            else
            {
                Debug.Log("login failed");
                if (OnSocialAuthenticateFailure != null)
                    OnSocialAuthenticateFailure();
            }
        });
    }
}
