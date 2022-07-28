using Microsoft.MixedReality.Toolkit.UX;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
#endif

public class Auth : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    Guid ClientId = new Guid("e30b2762-c795-46ce-a130-a0b4aed71372");
    string Authority = "organizations";
    string Resource = "api://439fac99-6ae4-4918-a73a-2222411d24f4";
#endif

    [Tooltip("Access Token")]
    [SerializeField]
    public string AccessToken;

    public string Username { get; protected set; }

    public async void Login()
    {
        Debug.Log("Login called");

#if ENABLE_WINMD_SUPPORT
        string URI = string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", 
            WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());
        Debug.Log("Redirect URI: " + URI);

       WebAccountProvider wap =
            await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", Authority);

        WebTokenRequest wtr = new WebTokenRequest(wap, string.Empty, ClientId.ToString());
        wtr.Properties.Add("resource", Resource);
        WebTokenRequestResult tokenResponse = null;
        try
        {
            tokenResponse = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(wtr);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        Debug.Log("Silent Token Response: " + tokenResponse.ResponseStatus.ToString());
        if (tokenResponse.ResponseError != null)
        {
            Debug.Log("Error Code: " + tokenResponse.ResponseError.ErrorCode.ToString());
            Debug.Log("Error Msg: " + tokenResponse.ResponseError.ErrorMessage.ToString());
            foreach (var errProp in tokenResponse.ResponseError.Properties)
            {
                Debug.Log($"Error prop: ({errProp.Key}, {errProp.Value})");
            }
        }
        
        if (tokenResponse.ResponseStatus == WebTokenRequestStatus.Success)
        {
            foreach (var resp in tokenResponse.ResponseData)
            {
                var name = resp.WebAccount.UserName;
                AccessToken = resp.Token;
                var account = resp.WebAccount;
                Username = account.UserName;
                Debug.Log($"Username = {Username}");
            }

            Debug.Log($"Access Token: {AccessToken}");
        }
#endif
    }
}
