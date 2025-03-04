using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class CharacterSelection : NetworkBehaviour
{
    [SerializeField] private Image playerCharacterImage; // Assign in Inspector
    [SerializeField] private Image otherPlayerImage; // Assign in Inspector
    [SerializeField] private Sprite[] characterSprites; // Assign character sprites in Inspector
    [SerializeField] private Button changeButton; // Assign button in Inspector

    private NetworkVariable<int> selectedCharacterIndex = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        if (IsOwner)
        {
            changeButton.onClick.AddListener(CycleCharacter);
        }

        selectedCharacterIndex.OnValueChanged += (oldValue, newValue) =>
        {
            UpdateCharacterUI();
        };

        UpdateCharacterUI();
    }

    private void CycleCharacter()
    {
        if (!IsOwner) return;

        int newIndex = (selectedCharacterIndex.Value + 1) % characterSprites.Length;
        selectedCharacterIndex.Value = newIndex;

        RequestCharacterChangeServerRpc(newIndex);
    }

    [ServerRpc]
    private void RequestCharacterChangeServerRpc(int newIndex)
    {
        selectedCharacterIndex.Value = newIndex;
    }

    private void UpdateCharacterUI()
    {
        if (playerCharacterImage == null || otherPlayerImage == null) return;

        playerCharacterImage.sprite = characterSprites[selectedCharacterIndex.Value];

        // Find another player and assign their character
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != OwnerClientId)
            {
                otherPlayerImage.sprite = characterSprites[selectedCharacterIndex.Value];
            }
        }
    }
}
