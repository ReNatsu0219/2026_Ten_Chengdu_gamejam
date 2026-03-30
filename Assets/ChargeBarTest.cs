using UnityEngine;

public class ChargeBarTest : MonoBehaviour
{
    float currentValue = 100f;
    [SerializeField] ChargeBar chargeBar;
    void Awake()
    {
        chargeBar.Initialize(currentValue, 100f);
    }
    void Update()
    {
        currentValue -= Time.deltaTime * 10f;
        chargeBar.UpdateStats(currentValue, 100f);
    }
}
