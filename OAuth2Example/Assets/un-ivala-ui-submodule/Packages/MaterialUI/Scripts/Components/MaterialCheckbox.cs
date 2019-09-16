//  Copyright 2016 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using UnityEngine;
using UnityEngine.UI;

namespace MaterialUI
{
    /// <summary>
    /// Component that controls the visuals of a checkbox-style toggle control.
    /// </summary>
    /// <seealso cref="MaterialUI.ToggleBase" />
    [ExecuteInEditMode]
    [AddComponentMenu("MaterialUI/Toggles/Material Checkbox", 100)]
    public class MaterialCheckbox : ToggleBase
    {
        /// <summary>
        /// The check graphic.
        /// </summary>
        [SerializeField]
        private Graphic m_CheckImage;
        /// <summary>
        /// The check graphic.
        /// </summary>
        public Graphic checkImage
        {
            get { return m_CheckImage; }
            set { m_CheckImage = value; }
        }

        /// <summary>
        /// The frame graphic.
        /// </summary>
        [SerializeField]
        private Graphic m_FrameImage;
        /// <summary>
        /// The frame graphic.
        /// </summary>
        public Graphic frameImage
        {
            get { return m_FrameImage; }
            set { m_FrameImage = value; }
        }

        /// <summary>
        /// The check rectTransform.
        /// </summary>
        private RectTransform m_CheckRectTransform;
        /// <summary>
        /// The check rectTransform.
        /// If null, automatically gets the transform of the check image, if one exists.
        /// </summary>
        public RectTransform checkRectTransform
        {
            get
            {
                if (m_CheckRectTransform == null)
                {
                    if (m_CheckImage != null)
                    {
                        m_CheckRectTransform = (RectTransform)m_CheckImage.transform;
                    }
                }
                return m_CheckRectTransform;
            }
        }

        /// <summary>
        /// The current size of the check rectTransform.
        /// </summary>
        private float m_CurrentCheckSize;
        /// <summary>
        /// The current size of the frame graphic.
        /// </summary>
        private Color m_CurrentFrameColor;

        /// <summary>
        /// See MonoBehaviour.OnEnable.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            // SS_BEGIN
            if ( checkImage != null )
            {                
                m_CheckRectTransform = checkImage.GetComponent<RectTransform>();
            }
            // SS_END
        }

        // SS_BEGIN
        public void Turn( bool bTurnOn )
        {
            if ( bTurnOn )
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }
        // SS_END

        /// <summary>
        /// Sets and animates the toggle to 'on'.
        /// Updates the toggle, ripple and graphic color and graphic data if applicable.
        /// </summary>
        public override void TurnOn()
        {
            // SS_BEGIN
            if ( checkImage != null )
            {                
                m_CurrentCheckSize = checkImage.rectTransform.sizeDelta.x;
                m_CurrentColor = checkImage.color;
            }
            if ( frameImage != null )
            {                
                m_CurrentFrameColor = frameImage.color;
            }
            // SS_END

            base.TurnOn();
        }

        /// <summary>
        /// Sets the toggle to 'on' without an animation.
        /// Updates the toggle, ripple and graphic color and graphic data if applicable.
        /// </summary>
        public override void TurnOnInstant()
        {
            base.TurnOnInstant();

            if (m_Toggle.interactable)
            {
                AnimOnComplete();
            }

            // SS_BEGIN
            if ( checkRectTransform != null )
            {                
                checkRectTransform.sizeDelta = new Vector2(24, 24);
            }
            // SS_END
        }

        /// <summary>
        /// Sets and animates the toggle to 'off'.
        /// Updates the toggle, ripple and graphic color and graphic data if applicable.
        /// </summary>
        public override void TurnOff()
        {
            // SS_BEGIN
            if ( checkImage != null )
            {                
                m_CurrentCheckSize = checkImage.rectTransform.sizeDelta.x;
                m_CurrentColor = checkImage.color;
            }
            if ( frameImage != null )
            {                
                m_CurrentFrameColor = frameImage.color;
            }
            // SS_END

            base.TurnOff();
        }

        /// <summary>
        /// Sets the toggle to 'off' without an animation.
        /// Updates the toggle, ripple and graphic color and graphic data if applicable.
        /// </summary>
        public override void TurnOffInstant()
        {
            base.TurnOffInstant();

            if (m_Toggle.interactable)
            {
                AnimOffComplete();
            }

            // SS_BEGIN
            if ( checkRectTransform != null )
            {                
                checkRectTransform.sizeDelta = Vector2.zero;
            }
            // SS_END
        }

        /// <summary>
        /// Makes the toggle object interactable, independant of the Toggle component's interactable value.
        /// Not to be confused with MonoBehaviour.OnEnable.
        /// </summary>
        public override void Enable()
        {
            base.Enable();

            if (m_Toggle.isOn)
            {
                AnimOnComplete();
            }
            else
            {
                AnimOffComplete();
            }
        }

        /// <summary>
        /// Makes the toggle object not interactable, independant of the Toggle component's interactable value.
        /// Not to be confused with MonoBehaviour.OnDisable.
        /// </summary>
        public override void Disable()
        {
            base.Disable();

            checkImage.color = disabledColor;
            frameImage.color = disabledColor;
        }

        /// <summary>
        /// Begins animating the toggle to the 'on' visual state.
        /// </summary>
        public override void AnimOn()
        {
            base.AnimOn();

            // SS_BEGIN
            if ( checkImage != null )
            {                
                checkImage.color = Tween.QuintOut(m_CurrentColor, onColor, m_AnimDeltaTime, animationDuration);
            }
            if ( frameImage != null )
            {                
                frameImage.color = Tween.QuintOut(m_CurrentFrameColor, onColor, m_AnimDeltaTime, animationDuration);
            }

            float tempSize = Tween.QuintOut(m_CurrentCheckSize, 24, m_AnimDeltaTime, animationDuration);

            if ( checkRectTransform != null )
            {
                checkRectTransform.sizeDelta = new Vector2(tempSize, tempSize);
            }
            // SS_END
        }

        /// <summary>
        /// Called when the toggle finishes animating to the 'on' visual state.
        /// </summary>
        public override void AnimOnComplete()
        {
            base.AnimOnComplete();
            // SS_BEGIN
            if ( checkImage != null )
            {                
                checkImage.color = onColor;
            }
            if ( frameImage != null )
            {                
                frameImage.color = onColor;
            }

            if ( checkRectTransform != null )
            {                
                checkRectTransform.sizeDelta = new Vector2(24, 24);
            }
            // SS_END
        }

        /// <summary>
        /// Begins animating the toggle to the 'off' visual state.
        /// </summary>
        public override void AnimOff()
        {
            base.AnimOff();
            // SS_BEGIN
            if ( checkImage != null )
            {                
                checkImage.color = Tween.QuintOut(m_CurrentColor, offColor, m_AnimDeltaTime, animationDuration);
            }
            if ( frameImage != null )
            {                
                frameImage.color = Tween.QuintOut(m_CurrentFrameColor, offColor, m_AnimDeltaTime, animationDuration);
            }

            float tempSize = Tween.QuintOut(m_CurrentCheckSize, 0, m_AnimDeltaTime, animationDuration);

            if ( checkRectTransform != null )
            {                
                checkRectTransform.sizeDelta = new Vector2(tempSize, tempSize);
            }
            // SS_END
        }

        /// <summary>
        /// Called when the toggle finishes animating to the 'off' visual state.
        /// </summary>
        public override void AnimOffComplete()
        {
            base.AnimOffComplete();
            // SS_BEGIN
            if (checkImage != null)
            {                
                checkImage.color = offColor;
            }
            if ( frameImage != null )
            {                
                frameImage.color = offColor;
            }

            if ( checkRectTransform != null )
            {                
                checkRectTransform.sizeDelta = new Vector2(0, 0);
            }
            // SS_END
        }

#if UNITY_EDITOR
        /// <summary>
        /// See MonoBehaviour.OnValidate.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_Interactable)
            {
                // SS_BEGIN
                if ( m_CheckImage != null )
                {
                    m_CheckImage.color = toggle.isOn ? m_OnColor : m_OffColor;
                }
                
                if ( m_FrameImage != null )
                {
                    m_FrameImage.color = toggle.isOn ? m_OnColor : m_OffColor;
                }
                // SS_END
            }
            else
            {
                m_CheckImage.color = m_DisabledColor;
                m_FrameImage.color = m_DisabledColor;
            }
        }
#endif
    }
}
