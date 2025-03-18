using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAppearance : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer characterSpriteRenderer; // Reference to SpriteRenderer
    [SerializeField] private List<Sprite> availableSprites; // List of available sprites
    [SerializeField] private Color[] spriteTints; // Optional: Apply color tints based on sprite


    void Start()
    {
        if (!IsOwner) return;

        int selectedSpriteIndex = PlayerPrefs.GetInt("PlayerSpriteIndex", 0); // Default to 0 if not found

        // Ensure the index is within range
        if (selectedSpriteIndex >= 0 && selectedSpriteIndex < availableSprites.Count)
        {
            characterSpriteRenderer.sprite = availableSprites[selectedSpriteIndex];

            // Optional: Apply a color tint based on the selected sprite
            if (selectedSpriteIndex < spriteTints.Length)
            {
                characterSpriteRenderer.color = spriteTints[selectedSpriteIndex];
            }
        }
    }

}
