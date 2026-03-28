using UnityEngine;

[CreateAssetMenu(menuName = "Config/PlayerConfig", fileName = "NewPlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [field: SerializeField] public float BaseSpeed { get; private set; } = 2f;
    [field: SerializeField] public PlayerAnimConfig AnimConfig { get; private set; }
}
