using UnityEngine;
using System.Collections;

public class LocalizationParams : MonoBehaviour {

	[SerializeField]
	private string m_LocalizationFile = "Localization";

	static private LocalizationParams m_Instance = null;

	void Awake()
	{
		if ( m_Instance )
		{
			Debug.LogWarning("LocalizationParams: singleton already exists");
		}
		m_Instance = this;
	}

	static public string localizationFileName
	{
		get{
			if ( m_Instance == null || string.IsNullOrEmpty(m_Instance.m_LocalizationFile) )
				return "Localization";
			return m_Instance.m_LocalizationFile;
		}
	}
}
