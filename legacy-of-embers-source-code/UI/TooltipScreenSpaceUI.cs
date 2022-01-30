using Game.Control;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Tooltip for in-game UI that dynamically changes based on text displayed.
    /// It is a singleton so that many scripts can call its functions
    /// statically.
    /// </summary>
    public class TooltipScreenSpaceUI : Singleton<TooltipScreenSpaceUI>
    {
        [SerializeField]
        [Tooltip("Player controller object in scene.")]
        private PlayerController PlayerController;

        [SerializeField]
        [Tooltip("Rect transform of the canvas that holds the " + 
            "TooltipScreenSpaceUI object.")]
        private RectTransform CanvasRectTransform;

        [SerializeField]
        [Tooltip("Transform containing the background component of the " + 
            "tootlip")]
        private RectTransform BackgroundTransform;

        [SerializeField]
        [Tooltip("The background image of the TooltipScreenSpaceUI object.")]
        private Image BackgroundImage;

        [SerializeField]
        [Tooltip("The text of the TooltipScreenSpaceUI object. Will " + 
            "display the actual tooltip text.")]
        private TextMeshProUGUI TooltipText;

        /// <summary>
        /// Rect transform of the entire TooltipScreenSpaceUI object.
        /// </summary>
        private RectTransform rectTransform;

        protected override void Awake()
        {
            base.Awake();
            rectTransform = GetComponent<RectTransform>();

            BackgroundImage.color = new Color(BackgroundImage.color.r,
                BackgroundImage.color.g, BackgroundImage.color.b, 0.0f);
            TooltipText.color = new Color(TooltipText.color.r, TooltipText.color.g,
                TooltipText.color.b, 0.0f);
            HideTooltip();
        }

        private void Update()
        {
            Vector2 anchoredPosition = PlayerController.MousePosition / CanvasRectTransform.localScale.x;

            if (anchoredPosition.x + BackgroundTransform.rect.width > CanvasRectTransform.rect.width)
            {
                anchoredPosition.x = CanvasRectTransform.rect.width - BackgroundTransform.rect.width;
            }
            if (anchoredPosition.y + BackgroundTransform.rect.height > CanvasRectTransform.rect.height)
            {
                anchoredPosition.y = CanvasRectTransform.rect.height - BackgroundTransform.rect.height;
            }

            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// Sets the text of the tooltip to the given text.
        /// </summary>
        /// <param name="tooltipText">Text to display in the tooltip.</param>
        private void SetText(string tooltipText)
        {
            TooltipText.SetText(tooltipText);
            TooltipText.ForceMeshUpdate();

            Vector2 textSize = TooltipText.GetRenderedValues(false);
            Vector2 paddingSize = new Vector2(8, 8);

            BackgroundTransform.sizeDelta = textSize + paddingSize;
        }

        /// <summary>
        /// Activates the tooltip object, triggering a fade in animation before
        /// it displays the tooltip text.
        /// </summary>
        /// <param name="tooltipText">Text to display in the tooltip.</param>
        public void ShowTooltip(string tooltipText)
        {
            gameObject.SetActive(true);
            SetText(tooltipText);
        }

        /// <summary>
        /// Deactivates the tooltip object, hiding it from view.
        /// </summary>
        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}

