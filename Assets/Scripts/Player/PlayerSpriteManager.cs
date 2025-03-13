using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEditor;

public class PlayerSpriteManager : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Assign in Inspector
    public List<Sprite[]> playerSpriteSheets; // Different color sprite sets
    public Animator animator; // Assign in Inspector

    private int selectedSpriteIndex = 0;
    private AnimatorOverrideController overrideController;

    void Start()
    {
        selectedSpriteIndex = PlayerPrefs.GetInt("SelectedSpriteIndex", 0);
        ApplySpriteSwap();
    }

    public void SetPlayerSprite(int index)
    {
        if (index >= 0 && index < playerSpriteSheets.Count)
        {
            selectedSpriteIndex = index;
            ApplySpriteSwap();

            // Save selection for the next scene
            PlayerPrefs.SetInt("SelectedSpriteIndex", selectedSpriteIndex);
            PlayerPrefs.Save();
        }
    }

    private void ApplySpriteSwap()
    {
        if (playerSpriteSheets.Count > selectedSpriteIndex)
        {
            Sprite[] newSprites = playerSpriteSheets[selectedSpriteIndex];

            // Swap all animation frames
            SwapAnimationSprites(newSprites);
        }
    }

    private void SwapAnimationSprites(Sprite[] newSprites)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator or Controller is missing!");
            return;
        }

        // Ensure we are using an AnimatorOverrideController
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        foreach (var clipPair in overrideController.clips)
        {
            AnimationClip originalClip = clipPair.originalClip;
            AnimationClip newClip = Instantiate(originalClip); // Create a new instance

            // Get sprite keyframes
            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(originalClip, 
                EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"));

            if (keyframes == null || keyframes.Length == 0)
            {
                Debug.LogError($"No sprite keyframes found in animation clip {originalClip.name}!");
                continue;
            }

            // Replace keyframe sprites
            for (int i = 0; i < keyframes.Length && i < newSprites.Length; i++)
            {
                keyframes[i].value = newSprites[i];
            }

            // Apply modified keyframes back to the animation clip
            AnimationUtility.SetObjectReferenceCurve(newClip, 
                EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keyframes);

            // Override the clip in the controller
            overrideController[originalClip] = newClip;
        }

        Debug.Log("Animation sprites swapped successfully!");
    }
}
