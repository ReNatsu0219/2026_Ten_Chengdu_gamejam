using UnityEngine;

public class PlayerReusableData
{
    public float StepTimer { get; set; } = 0f;
    public ECharacterFace PlayerFace { get; set; } = ECharacterFace.Front;
    public Vector2 InputDir { get; set; }
    public float SpeedMult { get; set; } = 1f;
}
