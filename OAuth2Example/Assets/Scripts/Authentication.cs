using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Authentication : MonoBehaviour {

	private readonly string base_url = "https://www.apphost.com";
	private string access_token = "";
	private readonly string client_id = "xxxxx-xxxxxx-xxxxxx-xxxxxxx-xxx";

	private string code = "";
	private string refreshToken = "";

	private readonly string tokensFilePath = "tokens.json";

	private bool dataAlreadyLoaded = false;

	WebViewObject webViewObject;

	[Serializable]
	private class Tokens {
		public string access_token;
		public string refresh_token;
		public string lastSuccessCheck;
	}

	enum MemberType {
		USER1,
		USER2
	}

	enum RequestType {
		GET,
		POST
	}

	private RequestType currentRequestType = RequestType.GET;
	private MemberType memberType;
	private UnityWebRequest currentRequest;

	private string requestJSONResponse = "";
	public string Code {

		get {
			return code;
		}
		set {
			code = value;
		}

	}

	public string UrlToIntercept {

		get {
			return urlToIntercept;
		}
		set {
			urlToIntercept = value;
		}

	}

	private string urlToIntercept = "";
	System.Guid userGUID;

	private string UUID;

	//datetime of last succesful tokens check
	private string lastSuccedCheck;

	//Max number of days during the user may login with no internet access.
	//Then, the internet is required for a review of the access & refresh tokens
	private const int MAX_DAYS_TILL_MANDATORY_CONNECTION = 30;

	private void Start () {


		CheckUserAccountStatus ();

	}

	#region public class 

	public void LoginUSER2 () {

		userGUID = System.Guid.NewGuid ();

		UUID = userGUID.ToString () + userGUID.ToString ();

		memberType = MemberType.USER2;

		string code_challenge = EncryptUUID (UUID);

		var url = base_url + "/openid/login/user2/?code_challenge=" + code_challenge +
			"&code_challenge_method=S256&redirect_uri=" + base_url + "/openid/complete/user2/";

		StartCoroutine (InitWebView (url));

		StartCoroutine (InterceptingDeepLink (memberType));

	}
	public void LoginUSER1 () {

		userGUID = System.Guid.NewGuid ();

		UUID = userGUID.ToString () + userGUID.ToString ();

		memberType = MemberType.USER1;

		string code_challenge = EncryptUUID (UUID); //HASH

		var url = base_url + "/openid/authorize?client_id=" + client_id +
			"&code_challenge=" + code_challenge +
			"&code_challenge_method=S256&redirect_uri=" + base_url + "/openid/complete/&response_type=code&scope=openid";

		StartCoroutine (InitWebView (url));

		StartCoroutine (InterceptingDeepLink (memberType));

	}

	#endregion

	#region data persistent
	/*
		Tokens saved at persistent data path:

		iOS: Application.persistentDataPath points to /var/mobile/Containers/Data/Application/<guid>/Documents.

		Android: Application.persistentDataPath points to /storage/emulated/0/Android/data/<packagename>/files		
		
		https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
		
	 */

	[Serializable]
	class tokensFile {
		public string theFile;
	}
	void GetNewTokens (string JSON) {

		Tokens _tokens = new Tokens ();
		_tokens = JsonUtility.FromJson<Tokens> (JSON);

		if (_tokens.refresh_token != null || _tokens.access_token != null) {

			requestJSONResponse = JSON;

			access_token = _tokens.access_token;
			refreshToken = _tokens.refresh_token;

		}
	}

	void ClearAuthenticationInfo () {

		SaveTokens ("{}");

	}

	bool ExpiredTimeSinceLastSuccedCheck () {

		if (lastSuccedCheck != null) {

			DateTime lastDate;

			if (!DateTime.TryParse (lastSuccedCheck, out lastDate)) return true;

			if ((int) (System.DateTime.Now - lastDate).Days >= MAX_DAYS_TILL_MANDATORY_CONNECTION) {
				ShowLoadingEffect (false);
				DisplayLoggingCanvas (true);

				MaterialUI.DialogManager.ShowAlert ("Time expired! :(  \n\nYou must login again.", "Connection Error", null);
				return true;
			}
			return false;
		} else {
			OnError ();
			return true;
		}

	}
	bool LoadTokens () {

		string path = Application.persistentDataPath + "/" + tokensFilePath;
		dataAlreadyLoaded = true;

		if (File.Exists (path)) {

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (path, FileMode.Open);
			tokensFile dataFile = (tokensFile) bf.Deserialize (file);
			file.Close ();
			string data = dataFile.theFile;

			// string data = File.ReadAllText(path);
			Tokens tokens = JsonUtility.FromJson<Tokens> (data);

			access_token = tokens.access_token;
			refreshToken = tokens.refresh_token;
			lastSuccedCheck = tokens.lastSuccessCheck;

			return (!access_token.Equals (""));

		} else {
			access_token = "";
			refreshToken = "";
			return false;
		}

	}

	void SaveTokens (string JSONresult) {

		if (JSONresult.Equals ("") || JSONresult == null) return;

		Tokens newTokens = new Tokens ();

		JsonUtility.FromJsonOverwrite (JSONresult, newTokens);

		newTokens.lastSuccessCheck = System.DateTime.Now.ToString ();

		string newJson = JsonUtility.ToJson (newTokens);

		string path = Application.persistentDataPath + "/" + tokensFilePath;

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (path, FileMode.Create);
		tokensFile current = new tokensFile ();
		current.theFile = newJson;
		bf.Serialize (file, current);
		file.Close ();

	}

	#endregion

	#region http requests
	void RefreshingRequest () {

		currentRequestType = RequestType.POST;

		if (currentRequest != null)
			currentRequest.Dispose ();
		this.StopAllCoroutines ();

		WWWForm form = new WWWForm ();

		form.headers.Add ("Cache-Control", "no-cache");
		//  form.headers.Add("Content-Type", "application/x-www-form-urlencoded");

		form.AddField ("client_id", client_id);
		//  form.AddField("client_secret" ,clientSecret);
		form.AddField ("grant_type", "refresh_token");
		form.AddField ("refresh_token", refreshToken);

		currentRequest = UnityWebRequest.Post (base_url + "/openid/token/", form);

		StartCoroutine (SendRequest ());

	}

	public void CheckUserAccountStatus () {

		if (!dataAlreadyLoaded) {
			if (!LoadTokens ()) {
				ShowLoadingEffect (false);
				DisplayLoggingCanvas (true);
				return;
			}
		}

		currentRequestType = RequestType.GET;

		currentRequest = new UnityWebRequest (base_url + "/openid/userinfo/?access_token=" + access_token);
		currentRequest.SetRequestHeader ("Cache-Control", "no-cache");
		StartCoroutine (SendRequest ());
	}

	void App2ExchangeCodePerTokenAccess () { //POST request to token endpoint to exchange code for access_token

		currentRequestType = RequestType.POST;

		if (currentRequest != null)
			currentRequest.Dispose ();
		this.StopAllCoroutines ();

		WWWForm form = new WWWForm ();

		form.headers.Add ("Cache-Control", "no-cache");
		//  form.headers.Add("Content-Type", "application/x-www-form-urlencoded");

		form.AddField ("client_id", client_id);
		form.AddField ("code_verifier", UUID);
		form.AddField ("code", Code);
		form.AddField ("grant_type", "authorization_code");
		form.AddField ("redirect_uri", base_url + "/openid/complete/user2/");
		form.AddField ("backend", "user2");

		currentRequest = UnityWebRequest.Post (base_url + "/openid/convert-code/", form);

		StartCoroutine (SendRequest ());
	}

	void App1ExchangeCodePerTokenAccess () { //POST request to token endpoint to exchange code for access_token

		currentRequestType = RequestType.POST;
		if (currentRequest != null)
			currentRequest.Dispose ();
		this.StopAllCoroutines ();

		WWWForm form = new WWWForm ();
		// form.headers.Clear();

		form.headers.Add ("Cache-Control", "no-cache");

		form.AddField ("client_id", client_id);
		form.AddField ("code_verifier", UUID);
		form.AddField ("code", Code);
		form.AddField ("grant_type", "authorization_code");
		form.AddField ("redirect_uri", base_url + "/openid/complete/");

		currentRequest = UnityWebRequest.Post (base_url + "/openid/token/", form);

		StartCoroutine (SendRequest ());
	}

	IEnumerator SendRequest () {

		using (currentRequest) {
			ShowLoadingEffect (true);

			currentRequest.timeout = 10;

			yield return currentRequest.SendWebRequest ();

			if (currentRequestType == RequestType.GET) {

				switch (System.Convert.ToInt16 (currentRequest.responseCode)) {

					case 401:
						//Try to refresh token
						RefreshingRequest ();

						break;

					case 200:
						//Store tokens and grant access
						SaveTokens (requestJSONResponse);
						StartCoroutine (LoadContent ());
						break;

					case 400:
						//clear authenticate info && display login screen
						ClearAuthenticationInfo ();
						ShowLoadingEffect (false);
						DisplayLoggingCanvas (true);

						break;

					default:

						if (!ExpiredTimeSinceLastSuccedCheck ()) {
							StartCoroutine (LoadContent ());

						}

						break;
				}

			} else {

				OnError ();

				if (!currentRequest.isHttpError && !currentRequest.isNetworkError) {

					Debug.LogWarning (currentRequest.downloadHandler.text);

					GetNewTokens (currentRequest.downloadHandler.text);

					currentRequest.Dispose ();

					currentRequest = null;

					CheckUserAccountStatus ();

				}
			}

			yield break;

		}
	}

	#endregion

	#region URL utils

	private string GetParam (Uri uri, string paramToExtract) {
		// Dictionary<String, String> query_pairs = new Dictionary<String, String>();
		string query = uri.Query;
		string[] pairs = query.Split ('&');
		foreach (string pair in pairs) {
			int idx = pair.IndexOf ("=");
			if (idx < 0) continue;
			// query_pairs.Add(pair.Substring(0, idx), pair.Substring(idx + 1));
			if (pair.Substring (0, idx - 1).Equals (paramToExtract)) {
				return pair.Substring (idx + 1);
			} else if (pair.Substring (1, idx - 1).Equals (paramToExtract)) {
				return pair.Substring (idx + 1);
			}
		}
		return "";
	}

	IEnumerator InterceptingDeepLink (MemberType member) {

		// fetchGetHttpRedirection();

		while (UrlToIntercept == null || !UrlToIntercept.ToLower ().Contains ("code=")) {
			yield return null;
		}

		Uri uri = new Uri (UrlToIntercept);

		Code = GetParam (uri, "code");

		urlToIntercept = "";

		switch (member) {

			case MemberType.USER1:
				App1ExchangeCodePerTokenAccess ();
				break;

			case MemberType.USER2:
				App2ExchangeCodePerTokenAccess ();
				break;

		}

		yield break;
	}

	bool webOpened = false;
	void HideWebView (bool value = true) {

		if (!value && webOpened) return;
		else if (!value && !webOpened) webOpened = true;

		webViewObject.SetVisibility (!value);
	}
	IEnumerator InitWebView (string _url) {

		webViewObject = (new GameObject ("WebViewObject")).AddComponent<WebViewObject> ();

		webViewObject.Init (
			cb: (msg) => {
				Debug.Log (string.Format ("CallFromJS[{0}]", msg));

			},
			err: (msg) => {
				Debug.Log (string.Format ("CallOnError[{0}]", msg));
				webOpened = false;
				HideWebView ();
				ShowLoadingEffect (false);
				DisplayLoggingCanvas (true);
				MaterialUI.DialogManager.ShowAlert ("Oops!. . . \n\n Something was wrong :( ", "Connection Error", null);

			},
			started: (msg) => {
				Debug.Log (string.Format ("CallOnStarted[{0}]", msg));

			},
			ld: (msg) => {
				HideWebView (false);

				Debug.Log (string.Format ("CallOnLoaded[{0}]", msg));
				if (msg.Contains ("code=") || msg.Contains ("error=")) {
					if (msg.Contains ("code=")) {
						DisplayLoggingCanvas (false);
					}
					UrlToIntercept = msg;
					webOpened = false;
					HideWebView ();
				}

#if UNITY_EDITOR_OSX || !UNITY_ANDROID
				// NOTE: depending on the situation, you might prefer
				// the 'iframe' approach.
				// cf. https://github.com/gree/unity-webview/issues/189
#if true
				webViewObject.EvaluateJS (@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
				webViewObject.EvaluateJS (@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#endif
				webViewObject.EvaluateJS (@"Unity.call('ua=' + navigator.userAgent)");

			},
			//ua: "custom user agent string",
			enableWKWebView : true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		webViewObject.bitmapRefreshCycle = 1;
#endif
		webViewObject.SetMargins (0, 0, 0, 0);
		ShowLoadingEffect (true);
		DisplayLoggingCanvas (false);

#if !UNITY_WEBPLAYER
		if (_url.StartsWith ("http")) {
			webViewObject.LoadURL (_url.Replace (" ", "%20"));
		} else {
			var exts = new string[] {
				".jpg",
				".js",
				".html" // should be last
			};
			foreach (var ext in exts) {
				var url = _url.Replace (".html", ext);
				var src = System.IO.Path.Combine (Application.streamingAssetsPath, url);
				var dst = System.IO.Path.Combine (Application.persistentDataPath, url);
				byte[] result = null;
				if (src.Contains ("://")) { // for Android
					var www = new WWW (src);
					yield return www;
					result = www.bytes;
				} else {
					result = System.IO.File.ReadAllBytes (src);
				}
				System.IO.File.WriteAllBytes (dst, result);
				if (ext == ".html") {
					webViewObject.LoadURL ("file://" + dst.Replace (" ", "%20"));
					break;
				}
			}
		}
#else
		if (_url.StartsWith ("http")) {
			webViewObject.LoadURL (_url.Replace (" ", "%20"));
		} else {
			webViewObject.LoadURL ("StreamingAssets/" + _url.Replace (" ", "%20"));
		}
		webViewObject.EvaluateJS (
			"parent.$(function() {" +
			"   window.Unity = {" +
			"       call:function(msg) {" +
			"           parent.unityWebView.sendMessage('WebViewObject', msg)" +
			"       }" +
			"   };" +
			"});");
#endif
		yield break;
	}

	string EncryptUUID (string UUID) {

		return UUID = OAuthUtils.GetSecureUUID (UUID);
	}

	#endregion
	void OnError () {

		if ((currentRequest.isHttpError || currentRequest.isNetworkError) &&

			System.Convert.ToInt16 (currentRequest.responseCode) != 401) {

			MaterialUI.DialogManager.ShowAlert (currentRequest.error, "ERROR", null);

			ShowLoadingEffect (false);
			DisplayLoggingCanvas (true);

		}

	}

	IEnumerator LoadContent () {

		ShowLoadingEffect (true);
		DisplayLoggingCanvas (false);
		// yield return new WaitForSeconds(1.5f);

		AsyncOperation loadContent = SceneManager.LoadSceneAsync ("main-scene", LoadSceneMode.Single);

		while (!loadContent.isDone) {
			yield return null;
		}

	}

	#region AuthenticationUIManager static class 

	void DisplayLoggingCanvas (bool ON) {
		AuthenticationUIManager.instance.ShowAthenticationScreen (ON);
	}
	void ShowLoadingEffect (bool ON) {
		AuthenticationUIManager.instance.ShowLoadingEffect (ON);
	}

	#endregion

}