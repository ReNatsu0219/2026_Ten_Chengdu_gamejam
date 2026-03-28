using UnityEngine;

public class PlayerReusableData
{
    public ECharacterFace PlayerFace { get; set; } = ECharacterFace.Front;
    public Vector2 InputDir { get; set; }
    public float SpeedMult { get; set; } = 1f;
}
