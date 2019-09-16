//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
// [SS] BEGIN
using UnityEngine.UI;
using TMPro;
// [SS] END

/// <summary>
/// Simple script that lets you localize a UIWidget.
/// </summary>

[ExecuteInEditMode]
// [SS] BEGIN Commented out to support uGUI and TextMeshPro
//[RequireComponent(typeof(UIWidget))]
// [SS] END
[AddComponentMenu("NGUI/UI/Localize")]
public class UILocalize : MonoBehaviour
{
	/// <summary>
	/// Localization key.
	/// </summary>

	public string key;

// [SS] BEGIN
	public string keyProperty 
	{ 
		set { 
			key = value;
			OnLocalize();
		} 

		get { 
			return key;
		} 
	}

	[SerializeField]
	private TextMeshProUGUI textMeshPro;

	[SerializeField]
	private Text text;
// [SS] END

	/// <summary>
	/// Manually change the value of whatever the localization component is attached to.
	/// </summary>

	public string value
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				// [SS] BEGIN
				bool bProcessed = false;
				if ( textMeshPro != null )
				{
					textMeshPro.text = value;
					bProcessed = true;
				}

				if ( !bProcessed )
				{					
					if ( text != null )
					{
						text.text = value;
					}
				}

				if ( !bProcessed )
				{					
					UIWidget w = GetComponent<UIWidget>();
					UILabel lbl = w as UILabel;
					UISprite sp = w as UISprite;
					
					if (lbl != null)
					{
						// If this is a label used by input, we should localize its default value instead
						UIInput input = NGUITools.FindInParents<UIInput>(lbl.gameObject);
						if (input != null && input.label == lbl) input.defaultText = value;
						else lbl.text = value;
						#if UNITY_EDITOR
						if (!Application.isPlaying) NGUITools.SetDirty(lbl);
						#endif
					}
					else if (sp != null)
					{
						UIButton btn = NGUITools.FindInParents<UIButton>(sp.gameObject);
						if (btn != null && btn.tweenTarget == sp.gameObject)
							btn.normalSprite = value;
						
						sp.spriteName = value;
						sp.MakePixelPerfect();
						#if UNITY_EDITOR
						if (!Application.isPlaying) NGUITools.SetDirty(sp);
						#endif
					}
				}				
				// [SS] END
			}
		}
	}

	bool mStarted = false;

	/// <summary>
	/// Localize the widget on enable, but only if it has been started already.
	/// </summary>

	void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mStarted) OnLocalize();
	}

    //[pk] mergeme begin
    void Awake()
    {
        Localization.onLocalize += OnLocalize;
    }
    //[pk] mergeme end

    /// <summary>
    /// Localize the widget on start.
    /// </summary>

    void Start ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		mStarted = true;
		OnLocalize();
	}

	/// <summary>
	/// This function is called by the Localization manager via a broadcast SendMessage.
	/// </summary>

	void OnLocalize ()
	{		
		// If no localization key has been specified, use the label's text as the key
		if (string.IsNullOrEmpty(key))
		{
			UILabel lbl = GetComponent<UILabel>();
			if (lbl != null) key = lbl.text;
		}

		// [SS] BEGIN
		if (string.IsNullOrEmpty(key))
		{
			key = gameObject.name;
		}
		// [SS] END

		// If we still don't have a key, leave the value as blank
		if (!string.IsNullOrEmpty(key)) value = Localization.Get(key);
	}
}
