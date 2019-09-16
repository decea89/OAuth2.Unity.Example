//  Copyright 2016 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using UnityEngine;
using UnityEngine.UI;

namespace MaterialUI
{
    [ExecuteInEditMode]
    [AddComponentMenu("MaterialUI/Material Radio", 100)]
    public class MaterialRadio : ToggleBase
    {
        [SerializeField]
        private Graphic m_DotImage;

        // [SS] BEGIN
        [SerializeField]
        private Vector2 m_DotImageSize = new Vector2(24, 24);
        // [SS] END

        [SerializeField]
        private Graphic m_RingImage;

        private float m_CurrentDotSize;

        public override void TurnOn()
        {
            m_CurrentDotSize = m_DotImage.rectTransform.sizeDelta.x;
            m_CurrentColor = m_DotImage.color;

            base.TurnOn();
        }

        public override void TurnOnInstant()
        {
            base.TurnOnInstant();

            if (toggle.interactable)
            {
                m_DotImage.color = m_OnColor;
                m_RingImage.color = m_OnColor;
            }

            // [SS] BEGIN
            m_DotImage.rectTransform.sizeDelta = m_DotImageSize;
            // [SS] END
        }

        public override void TurnOff()
        {
            m_CurrentDotSize = m_DotImage.rectTransform.sizeDelta.x;
            m_CurrentColor = m_DotImage.color;

            base.TurnOff();
        }

        public override void TurnOffInstant()
        {
            base.TurnOffInstant();

            if (toggle.interactable)
            {
                m_DotImage.color = m_OffColor;
                m_RingImage.color = m_OffColor;
            }

            m_DotImage.rectTransform.sizeDelta = Vector2.zero;
        }

        public override void Enable()
        {
            if (toggle.isOn)
            {
                m_DotImage.color = m_OnColor;
                m_RingImage.color = m_OnColor;
            }
            else
            {
                m_DotImage.color = m_OffColor;
                m_RingImage.color = m_OffColor;
            }

            base.Enable();
        }

        public override void Disable()
        {
            m_DotImage.color = m_DisabledColor;
            m_RingImage.color = m_DisabledColor;

            base.Disable();
        }

        public override void AnimOn()
        {
            base.AnimOn();

            m_DotImage.color = Tween.QuintOut(m_CurrentColor, m_OnColor, m_AnimDeltaTime, m_AnimationDuration);
            m_RingImage.color = m_DotImage.color;

            // [SS] BEGIN
            float tempSize = Tween.QuintOut(m_CurrentDotSize, m_DotImageSize.x, m_AnimDeltaTime, m_AnimationDuration);
            // [SS] END

            m_DotImage.rectTransform.sizeDelta = new Vector2(tempSize, tempSize);
        }

        public override void AnimOnComplete()
        {
            base.AnimOnComplete();

            m_DotImage.color = m_OnColor;
            m_RingImage.color = m_OnColor;

            // [SS] BEGIN
            m_DotImage.rectTransform.sizeDelta = m_DotImageSize;
            // [SS] END
        }

        public override void AnimOff()
        {
            base.AnimOff();

            m_DotImage.color = Tween.QuintOut(m_CurrentColor, m_OffColor, m_AnimDeltaTime, m_AnimationDuration);
            m_RingImage.color = m_DotImage.color;

            float tempSize = Tween.QuintOut(m_CurrentDotSize, 0, m_AnimDeltaTime, m_AnimationDuration);

            m_DotImage.rectTransform.sizeDelta = new Vector2(tempSize, tempSize);
        }

        public override void AnimOffComplete()
        {
            base.AnimOffComplete();

            m_DotImage.color = m_OffColor;
            m_RingImage.color = m_OffColor;

            m_DotImage.rectTransform.sizeDelta = Vector2.zero;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
// [SS] BEGIN Checked m_DotImage and m_RingImage on null
            if (m_Interactable)
            {
				if ( m_DotImage != null )
					m_DotImage.color = toggle.isOn ? m_OnColor : m_OffColor;
				if ( m_RingImage != null )
					m_RingImage.color = toggle.isOn ? m_OnColor : m_OffColor;
            }
            else
            {
				if ( m_DotImage != null )
					m_DotImage.color = m_DisabledColor;
				if ( m_RingImage != null )
					m_RingImage.color = m_DisabledColor;
            }
// [SS] END
        }
#endif
    }
}
