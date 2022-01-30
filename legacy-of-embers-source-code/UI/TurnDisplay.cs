using Game.Building;
using Game.Combat;
using Game.Core;
using Game.Entity;
using Game.Movement;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// UI Object that tracks the sequence of actors that are taking turns.
    /// </summary>
    public class TurnDisplay : MonoBehaviour
    {
        [Header("Turn Display UI Images")]

        [Tooltip("Images that represent each actor in the turn sequence.")]
        public Image[] ActorThumbnails;

        [Header("Turn Data Display Objects")]

        [Tooltip("Collection of UI objects that together indicate how much " +
            "time is remaining in a turn (move time).")]
        public GameObject TimeDisplay;

        [Tooltip("Collection of UI objects that together indicate how many " +
            "attacks are remaining in a turn.")]
        public GameObject AttackCountDisplay;

        // [Tooltip("Collection of UI objects that together indicate how many " +
        //     "bots have been built by an actor.")]
        //public GameObject BuildCountDisplay;

        [Header("Turn Data Display Fields")]

        [Tooltip("Indicates how much time is remaining in a turn (move time).")]
        public Image TimeImage;

        [Tooltip("Indicates how many attacks the actor has remaining in a " +
            "turn.")]
        public Image AttackImage;

        [Tooltip("Indicates how many bots the actor has built during the " +
            "game.")]
        public TextMeshProUGUI BuildCountText;

        [Header("Actor Alignment Border Colors")]

        [Tooltip("Color to signal player alignment.")]
        public Color AlignmentColorPlayer;

        [Tooltip("Color to signal enemy alignment.")]
        public Color AlignmentColorEnemy;

        [Tooltip("Color to signal no alignment.")]
        public Color AlignmentColorDefault;

        [Header("Buttons")]
        [Tooltip("Button that, when clicked, signals the end of the player " +
            "turn.")]
        public Button EndTurnButton;

        [Header("Sprites")]
        [Tooltip("Spite to use in the Turn Display when there is no actor to " +
            "fill a turn square.")]
        public Sprite DefaultSprite;

        [Header("Turn Indicator")]
        public CanvasGroup TurnIndicatorGroup;
        public TextMeshProUGUI IndicatorText;

        [SerializeField]
        public int blinkCount;

        [SerializeField]
        public float timeBetweenBlinks;

        public void Start()
        {
            EndTurnButton.onClick.AddListener(TurnManager.Instance.MoveToNextTurn);
        }

        private void OnEnable()
        {
            Builder.UpdateBuildCount += UpdateBuildCount;
            Fighter.UpdateAttackCount += UpdateAttackCount;
            Mover.UpdateMoveTime += UpdateMoveTime;

            TurnManager.Instance.TurnSequenceUpdated +=
                UpdateTurnSequenceDisplay;
            TurnManager.Instance.StartNewTurn += ShowTurnIndicator;
        }

        private void OnDisable()
        {
            Mover.UpdateMoveTime -= UpdateMoveTime;
            Fighter.UpdateAttackCount -= UpdateAttackCount;
            Builder.UpdateBuildCount -= UpdateBuildCount;
        }

        /// <summary>
        /// Calls the static instance of the TooltipScreenSpaceUI object to 
        /// display the summary of an actor when the player hovers over an image
        /// in the Turn Display.
        /// </summary>
        /// <param name="actorIndex">Which actor the player is currently
        /// hovering over.</param>
        public void ShowActorTooltip(int actorIndex)
        {
            if (actorIndex < TurnManager.Instance.TurnQueue.Count)
            {
                Actor currentActor = TurnManager.Instance.TurnQueue[actorIndex];
                if (currentActor != null)
                {
                    TooltipScreenSpaceUI.Instance.ShowTooltip(currentActor.ActorId.GetActorSummary());
                }
                else
                {
                    TooltipScreenSpaceUI.Instance.ShowTooltip("Empty.");
                }
            }
            else
            {
                TooltipScreenSpaceUI.Instance.ShowTooltip("Empty.");
            }

        }

        /// <summary>
        /// Calls the static instance of the TooltipScreenUI object to hide the 
        /// tooltip.
        /// </summary>
        public void HideTooltip()
        {
            TooltipScreenSpaceUI.Instance.HideTooltip();
        }

        /// <summary>
        /// Show canvas group that tells the player who's turn it is.
        /// </summary>
        public void ShowTurnIndicator()
        {
            StartCoroutine(ShowTurnIndicatorRoutine());
        }

        /// <summary>
        /// Routine to show canvas group that tells the player who's turn it is.
        /// </summary>
        public IEnumerator ShowTurnIndicatorRoutine()
        {
            IndicatorText.text = 
                TurnManager.Instance.TurnQueue[0].ActorId.ActorName + " Turn";
            for (int i = 0; i < blinkCount; i++)
            {
                TurnIndicatorGroup.alpha = 1;
                yield return new WaitForSeconds(timeBetweenBlinks);
                TurnIndicatorGroup.alpha = 0;
                yield return new WaitForSeconds(timeBetweenBlinks);
            }
            TurnIndicatorGroup.alpha = 0;
        }

        /// <summary>
        /// Updates the attack count in response to changes in the Fighter
        /// class.
        /// </summary>
        /// <param name="newCount">The new number of bots built by the
        /// builder.</param>
        private void UpdateAttackCount(int newCount)
        {
            if (TurnManager.Instance.TurnQueue[0].ActorId.ActorType == 
                ActorType.Player)
            {
                switch (newCount)
                {
                    case 3:
                        AttackImage.fillAmount = 1.0f;
                        break;
                    case 2:
                        AttackImage.fillAmount = 0.66f;
                        break;
                    case 1:
                        AttackImage.fillAmount = 0.33f;
                        break;
                    case 0:
                        AttackImage.fillAmount = 0.00f;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the build count in response to changes in the Builder
        /// class. Currently, there is no build count indicator. This will be
        /// fixed in a later patch.
        /// </summary>
        /// <param name="newCount">The new number of bots built by the
        /// builder.</param>
        private void UpdateBuildCount(int newCount)
        {
            //BuildCountText.text = newCount.ToString();
        }

        /// <summary>
        /// Updates the move time display in response to changes in the Mover
        /// class.
        /// </summary>
        /// <param name="newTime">The new move time the mover has in its
        /// turn.</param>
        private void UpdateMoveTime(float maxTime, float newTime)
        {
            if (TurnManager.Instance.TurnQueue[0].ActorId.ActorType == 
                ActorType.Player)
            {
                float fillPercentage = newTime / maxTime;
                TimeImage.fillAmount = fillPercentage;
            }
        }

        /// <summary>
        /// Updates the pictures of the Turn Display to represent the current 
        /// actor taking a turn and its sequential actors.
        /// </summary>
        private void UpdateTurnSequenceDisplay()
        {
            for (int i = 0; i < ActorThumbnails.Length; i++)
            {
                if (i < TurnManager.Instance.TurnQueue.Count)
                {
                    ActorThumbnails[i].sprite =
                        TurnManager.Instance.TurnQueue[i].ActorId.Thumbnail;
                    Alignment alignment =
                        TurnManager.Instance.TurnQueue[i].ActorId.Alignment;
                    Color alignmentColor = alignment == Alignment.Player ?
                        AlignmentColorPlayer : AlignmentColorEnemy;
                }
                else
                {
                    ActorThumbnails[i].sprite = DefaultSprite;
                }
            }
        }


    }
}
