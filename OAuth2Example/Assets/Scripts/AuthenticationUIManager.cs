using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthenticationUIManager : MonoBehaviour {

	// Use this for initialization

	public static AuthenticationUIManager instance;

	public GameObject AuthenticationScreen;

	public GameObject ivalaSign;
	public GameObject vinSign;

	public GameObject ivalaSignUIContainer;
	public GameObject vinSignUIContainer;	

	public GameObject retrySignUIContainer;	

	public GameObject loadingEffect;

	public CanvasGroup authCanvasGroup;

	private void Awake() {
		instance = this;
	}

	public void ShowAthenticationScreen(bool ON){
		
		retrySignUIContainer.SetActive(false);
		ivalaSignUIContainer.SetActive(true);
		vinSignUIContainer.SetActive(true);		
		ivalaSign.SetActive(ON);
		vinSign.SetActive(ON);
		DisableAuthCanvas(!ON);

	}
	public void ShowLoadingEffect(bool ON){
		loadingEffect.SetActive(ON);
	}	

	public void DisableAuthCanvas(bool ON){
		if(ON){
			authCanvasGroup.interactable = false;
			// authCanvasGroup.alpha = 0.95f;
		}else{
			authCanvasGroup.interactable = true;
			// authCanvasGroup.alpha = 1f;
		}

	}	
	public void ShowReTryMockup(bool ON){

		ivalaSignUIContainer.SetActive(!ON);
		vinSignUIContainer.SetActive(!ON);
		retrySignUIContainer.SetActive(ON);	
		DisableAuthCanvas(!ON);
	
	}		
	
}
