using Game.Inventories;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Tracks the changes in the player's inventory and updates UI accoridngly.
    /// </summary>
    public class InventoryDisplay : MonoBehaviour
    {
        [Tooltip("Inventory for the InventoryUIManager to track.")]
        public Inventory playerInventory;

        [Header("Inventory Counts")]

        [Tooltip("TMP text to represent the count of Cyan Crystals in the " +
            "player's inventory.")]
        public TextMeshProUGUI CyanCrystalCount;

        [Tooltip("TMP text to represent the count of Magenta Crystals in the " +
            "player's inventory.")]
        public TextMeshProUGUI MagentaCrystalCount;

        [Tooltip("TMP text to represent the count of Yellow Crystals in the " +
            "player's inventory.")]
        public TextMeshProUGUI YellowCrystalCount;

        [Tooltip("TMP text to represent the count of Steel in the " +
            "player's inventory.")]
        public TextMeshProUGUI SteelCount;

        [Tooltip("TMP text to represent the count of Wood in the " +
            "player's inventory.")]
        public TextMeshProUGUI WoodCount;

        /// <summary>
        /// Represents the current number of each resource in the subject 
        /// inventory.
        /// </summary>
        private int[] currentInventoryCounts;

        /// <summary>
        /// Array to store the resource counting text objects for convenient 
        /// access.
        /// </summary>
        private TextMeshProUGUI[] countUITextObjects;

        private void OnEnable()
        {
            currentInventoryCounts = new int[5];
            countUITextObjects = new TextMeshProUGUI[5]
            {
                CyanCrystalCount,
                MagentaCrystalCount,
                YellowCrystalCount,
                SteelCount,
                WoodCount
            };

            playerInventory.changedResourceCount += UpdateInventoryUI;
        }

        private void OnDisable()
        {
            playerInventory.changedResourceCount -= UpdateInventoryUI;
        }

        /// <summary>
        /// Updates the player's heads up display with new inventory counts.
        /// Responds to the player inventory's "updatedInventory" delegate.
        /// </summary>
        /// <param name="newInventoryCounts">An array depicting the new counts
        /// of all inventory items.</param>
        private void UpdateInventoryUI(int resourceId, int newCount)
        {
            int delta = newCount - currentInventoryCounts[resourceId];
            currentInventoryCounts[resourceId] += delta;
            StartCoroutine(UpdateCountRoutine(resourceId, delta));
        }

        /// <summary>
        /// Shows the change in inventory count before setting the count.
        /// </summary>
        /// <param name="delta">Change in inventory count to be displayed
        /// before new count.</param>
        /// <param name="newCount">New count to be displayed.</param>
        private IEnumerator UpdateCountRoutine(int resourceId, int delta)
        {
            TextMeshProUGUI textToUpdate = countUITextObjects[resourceId];
            Color deltaTextColor;
            string prefix;

            if (delta > 0)
            {
                prefix = "+";
                deltaTextColor = Color.cyan;
            }
            else if (delta < 0)
            {
                prefix = "";
                deltaTextColor = Color.magenta;
            }
            else
            {
                yield break;
            }

            textToUpdate.text = prefix + delta;
            textToUpdate.color = deltaTextColor;
            yield return new WaitForSeconds(1.0f);
            textToUpdate.color = Color.white;
            textToUpdate.text = currentInventoryCounts[resourceId].ToString();
        }
    }
}
