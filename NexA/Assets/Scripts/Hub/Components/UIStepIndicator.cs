using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Single step indicator UI component (background + text)
    /// Manages its own visual state (active, inactive, completed)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIStepIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI text;

        [Header("Colors")]
        [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 0.86f); // #3498DB
        [SerializeField] private Color inactiveColor = new Color(0.31f, 0.31f, 0.31f); // #505050
        [SerializeField] private Color completedColor = new Color(0.18f, 0.8f, 0.44f); // #2ECC71
        [SerializeField] private Color activeTextColor = Color.white;
        [SerializeField] private Color inactiveTextColor = new Color(0.5f, 0.5f, 0.5f); // #808080

        private StepState currentState = StepState.Inactive;

        /// <summary>
        /// Step indicator state
        /// </summary>
        public enum StepState
        {
            Inactive,   // Step not reached yet (grey)
            Active,     // Current step (blue)
            Completed   // Step already validated (green)
        }

        private void Awake()
        {
            if (background == null)
                background = GetComponentInChildren<Image>();
            
            if (text == null)
                text = GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Set the step text (e.g., "1. Username", "2. Email")
        /// </summary>
        public void SetText(string stepText)
        {
            if (text != null)
                text.text = stepText;
        }

        /// <summary>
        /// Set the step state with animated transition
        /// </summary>
        public void SetState(StepState newState, bool animate = true)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            Color targetBackgroundColor;
            Color targetTextColor;

            switch (newState)
            {
                case StepState.Active:
                    targetBackgroundColor = activeColor;
                    targetTextColor = activeTextColor;
                    break;

                case StepState.Completed:
                    targetBackgroundColor = completedColor;
                    targetTextColor = activeTextColor;
                    break;

                default: // StepState.Inactive
                    targetBackgroundColor = inactiveColor;
                    targetTextColor = inactiveTextColor;
                    break;
            }

            if (animate)
            {
                // Smooth color transition
                if (background != null)
                    AnimationHelper.FadeColor(background, targetBackgroundColor, AnimationHelper.NORMAL);
                
                if (text != null)
                    AnimationHelper.FadeColor(text, targetTextColor, AnimationHelper.NORMAL);
            }
            else
            {
                // Instant update
                if (background != null)
                    background.color = targetBackgroundColor;
                
                if (text != null)
                    text.color = targetTextColor;
            }
        }

        /// <summary>
        /// Get the current state
        /// </summary>
        public StepState GetState()
        {
            return currentState;
        }

        /// <summary>
        /// Reset to inactive state
        /// </summary>
        public void ResetState()
        {
            SetState(StepState.Inactive, animate: false);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Auto-find components in editor
        /// </summary>
        private void Reset()
        {
            background = GetComponentInChildren<Image>();
            text = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}
