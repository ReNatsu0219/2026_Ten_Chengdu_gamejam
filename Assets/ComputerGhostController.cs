using UnityEngine;

public class ComputerGhostController : MonoBehaviour
{
    [Header("显示对象")]
    [SerializeField] private GameObject ghostVisual;

    [Header("触发参数")]
    [SerializeField] private int killTickThreshold = 8;
    [SerializeField] private string deathReason = "Killed by Computer Ghost";

    [Header("运行时状态")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private int currentTickCount = 0;

    private void Awake()
    {
        if (ghostVisual == null)
            ghostVisual = gameObject;

        ghostVisual.SetActive(false);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void ActivateGhost()
    {
        if (isActive) return;

        if (ghostVisual == null) return;

        ghostVisual.SetActive(true);

        isActive = true;
        currentTickCount = 0;

        Subscribe();
    }

    public void DeactivateGhost()
    {
        isActive = false;
        currentTickCount = 0;

        if (ghostVisual != null)
            ghostVisual.SetActive(false);

        Unsubscribe();
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (NightManager.Instance != null)
            NightManager.Instance.OnQuickTick += HandleQuickTick;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightClear += DeactivateGhost;
            GameManager.Instance.OnPlayerDead += DeactivateGhost;
            GameManager.Instance.OnNightEnded += DeactivateGhost;
        }
    }

    private void Unsubscribe()
    {
        if (NightManager.Instance != null)
            NightManager.Instance.OnQuickTick -= HandleQuickTick;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightClear -= DeactivateGhost;
            GameManager.Instance.OnPlayerDead -= DeactivateGhost;
            GameManager.Instance.OnNightEnded -= DeactivateGhost;
        }
    }

    private void HandleQuickTick()
    {
        if (ghostVisual == null || !ghostVisual.activeInHierarchy)
        {
            DeactivateGhost();
            return;
        }

        if (!isActive) return;

        if (!IsComputerScreenStillOpen())
        {
            DeactivateGhost();
            return;
        }

        currentTickCount++;

        if (currentTickCount >= killTickThreshold)
        {
            if (IsComputerScreenStillOpen())
            {
                GameManager.Instance.PlayerDead(deathReason);
            }
        }
    }

    private bool IsComputerScreenStillOpen()
    {
        if (UIMgr.Instance == null) return false;
        return UIMgr.Instance.IsScreenPanelOpen();
    }
}