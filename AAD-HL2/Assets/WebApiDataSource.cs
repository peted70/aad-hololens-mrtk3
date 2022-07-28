using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebApiDataSource : DataSourceGOBase
{
    public delegate void RequestSuccessDelegate(string jsonText, object requestRef);
    public delegate void RequestFailureDelegate(string errorString, object requestRef);

    public DataSourceJson DataSourceJson { get { return DataSource as DataSourceJson; } }

    [Tooltip("URL for a custom Web API")]
    [SerializeField]
    private string url = "https://aad-hololens-api.azurewebsites.net/weatherforecast";

    [Tooltip("Auth data")]
    [SerializeField]
    private Auth auth;

    [Tooltip("Dialog Prefab")]
    [SerializeField]
    private Dialog DialogPrefabSmall;

    /// <summary>
    /// Set the text that will be parsed and used to build the memory based DOM.
    /// </summary>
    /// <param name="jsonText">THe json string to parse.</param>
    public void SetJson(string jsonText)
    {
        DataSourceJson.UpdateFromJson(jsonText);
    }

    /// <inheritdoc/>
    public override IDataSource AllocateDataSource()
    {
        return new DataSourceJson();
    }

    public void FetchData()
    {
        if (string.IsNullOrEmpty(auth.AccessToken))
        {
            Dialog.InstantiateFromPrefab(DialogPrefabSmall,
                new DialogProperty("Error", "No token to call the API with.",
                DialogButtonHelpers.OK), true, true);
        }

        StartCoroutine(StartJsonRequest(url, auth.AccessToken));
    }
    
    public IEnumerator StartJsonRequest(string uri, string accessToken, 
        RequestSuccessDelegate successDelegate = null, RequestFailureDelegate failureDelegate = null, 
        object requestRef = null)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
                
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || 
                webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
                if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
            {
                if (failureDelegate != null)
                {
                    failureDelegate.Invoke(webRequest.error, requestRef);
                }
            }
            else
            {
                string jsonText = webRequest.downloadHandler.text;

                DataSourceJson.UpdateFromJson(jsonText);
                if (successDelegate != null)
                {
                    successDelegate.Invoke(jsonText, requestRef);
                }
            }
        }
    }
}
