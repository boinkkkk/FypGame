using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAppearance : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer characterSpriteRenderer; // Reference to SpriteRenderer
    [SerializeField] private List<Sprite> availableSprites; // List of available sprites
    [SerializeField] private Color[] spriteTints; // Optional: Apply color tints based on sprite

    private NetworkVariable<int> spriteIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> colorIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    void Start()
    {
        if (IsOwner)
        {
            int savedSpriteIndex = PlayerPrefs.GetInt("PlayerSpriteIndex", 0);
            int savedColorIndex = Mathf.Clamp(savedSpriteIndex, 0, spriteTints.Length - 1); // Ensure it's in range

            spriteIndex.Value = savedSpriteIndex;
            colorIndex.Value = savedColorIndex;

            PlayerPrefs.SetInt("PlayerSpriteIndex", savedSpriteIndex);
            PlayerPrefs.Save();
        }

        // Apply the sprite and color when values change
        spriteIndex.OnValueChanged += ApplyAppearance;
        colorIndex.OnValueChanged += ApplyAppearance;

        ApplyAppearance(spriteIndex.Value, spriteIndex.Value); // Apply initially
    }

    private void ApplyAppearance(int oldValue, int newValue)
    {
        if (spriteIndex.Value >= 0 && spriteIndex.Value < availableSprites.Count)
        {
            characterSpriteRenderer.sprite = availableSprites[spriteIndex.Value];
        }

        if (colorIndex.Value >= 0 && colorIndex.Value < spriteTints.Length)
        {
            characterSpriteRenderer.color = spriteTints[colorIndex.Value];
        }
    }
    // void Start()
    // {
    //     if (!IsOwner) return;

    //     int selectedSpriteIndex = PlayerPrefs.GetInt("PlayerSpriteIndex", 0); // Default to 0 if not found

    //     // Ensure the index is within range
    //     if (selectedSpriteIndex >= 0 && selectedSpriteIndex < availableSprites.Count)
    //     {
    //         characterSpriteRenderer.sprite = availableSprites[selectedSpriteIndex];

    //         // Optional: Apply a color tint based on the selected sprite
    //         if (selectedSpriteIndex < spriteTints.Length)
    //         {
    //             characterSpriteRenderer.color = spriteTints[selectedSpriteIndex];
    //         }
    //     }
    // }

}
