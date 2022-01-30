using System.Collections.Generic;
using UnityEngine;

namespace Game.Graphics
{
    /// <summary>
    /// Randomizes the appearance of Humanoid Graphics to keep things fresh.
    /// </summary>
    public class AppearanceRandomizer : MonoBehaviour
    {
        [Tooltip("Determines if the look of this humanoid will be randomized.")]
        public bool Randomize = true;

        [Header("Alignment")]

        [Tooltip("Determines if this humanoid is an enemy and should " + 
            "thus wear a warm-colored shirt.")]
        public bool Enemy = false;

        [Header("Shirt")]

        [Tooltip("List of colors that an enemy humanoid's shirt can be.")]
        public List<Color> EnemyShirtColors;

        [Tooltip("List of colors that an friendly humanoid's shirt can be.")]
        public List<Color> FriendlyShirtColors;

        [Tooltip("Mesh renderer of the humanoid's shirt.")]
        public SkinnedMeshRenderer Shirt;

        [Header("Skin")]

        [Tooltip("List of available skin colors for humanoids.")]
        public List<Color> SkinColors;

        [Tooltip("Mesh renderer of all body parts exposed to be skin colored.")]
        public SkinnedMeshRenderer[] BodyParts;

        [Header("Hair")]

        [Tooltip("List of available hair colors for humanoids.")]
        public List<Color> HairColors;

        [Tooltip("Mesh renderer of all body parts exposed to be hair colored.")]
        public SkinnedMeshRenderer[] HairParts;

        private void Start()
        {
            if (Randomize)
            {
                SkinColors.Add(BodyParts[0].material.color);
                HairColors.Add(HairParts[0].material.color);
                FriendlyShirtColors.Add(Shirt.material.color);

                Color skinColor = SkinColors[Random.Range(0, SkinColors.Count)];
                foreach (SkinnedMeshRenderer meshRenderer in BodyParts)
                {
                    meshRenderer.material.color = skinColor;
                }

                Color hairColor = HairColors[Random.Range(0, HairColors.Count)];
                foreach (SkinnedMeshRenderer meshRenderer in HairParts)
                {
                    meshRenderer.material.color = hairColor;
                }

                if (Enemy)
                {
                    Shirt.material.color =
                        EnemyShirtColors[Random.Range(0, EnemyShirtColors.Count)];
                }
                else
                {
                    Shirt.material.color =
                        FriendlyShirtColors[Random.Range(0, FriendlyShirtColors.Count)];
                }
            }
        }
    }
}
