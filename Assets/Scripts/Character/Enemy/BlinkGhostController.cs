using UnityEngine;

public class BlinkGhostController : MonoBehaviour
{
    private enum GhostState
    {
        Inactive,
        WaitingSpawn,
        Active,
        Warning
    }

    [Header("引用")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D hitCollider;

    [Header("房间合法区域")]
    [SerializeField] private Collider2D roomArea;

    [Header("灯设施")]
    [SerializeField] private Facilitybase lightFacility;

    [Header("生成参数")]
    [SerializeField] private float minSpawnDistanceFromPlayer = 3.5f;
    [SerializeField] private int spawnTryCount = 30;
    [SerializeField] private float spawnDelaySeconds = 3f;

    [Header("刷新概率")]
    [SerializeField] private float baseSpawnChance = 0.12f;
    [SerializeField] private float spawnChancePerDay = 0.05f;
    [SerializeField] private float spawnChanceByNightProgress = 0.35f;
    [SerializeField] private float spawnChanceReducePerPower = 0.04f;

    [Header("瞬移基础参数")]
    [SerializeField] private float baseBlinkStepMin = 1.0f;
    [SerializeField] private float baseBlinkStepMax = 1.8f;
    [SerializeField] private int blinkEveryDarkCount = 2;

    [Header("斩杀参数")]
    [SerializeField] private float killRange = 1.0f;
    [SerializeField] private float warningStopDistance = 1.4f;
    [SerializeField] private float warningEscapeDistance = 1.2f;

    [Header("闪烁基础参数")]
    [SerializeField] private int baseFlickerTotalTicks = 50;
    [SerializeField] private int baseFlickerSwitchTicks = 2;

    [Header("数值成长参数")]
    [SerializeField] private int flickerTicksPerDay = 2;
    [SerializeField] private float flickerTicksByNightProgress = 6f;
    [SerializeField] private int flickerTicksReducePerPower = 2;

    [SerializeField] private int switchTicksAddPerPower = 1;
    [SerializeField] private float switchTicksReduceByNightProgress = 1f;
    [SerializeField] private int switchTicksReducePer2Day = 1;

    [SerializeField] private float blinkDistancePerDay = 0.10f;
    [SerializeField] private float blinkDistanceByNightProgress = 0.4f;
    [SerializeField] private float blinkDistanceReducePerPower = 0.10f;

    [Header("灯闪随机参数")]
    [SerializeField] private float darkChance = 0.85f;
    [SerializeField] private float brightChance = 1f;

    [Header("调试")]
    [SerializeField] private bool autoStartForTest = false;

    private GhostState currentState = GhostState.Inactive;

    private float currentBlinkStepMin;
    private float currentBlinkStepMax;

    private float spawnDelayTimer = 0f;
    private int darkCountSinceSpawn = 0;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider2D>();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        SetInactiveState();
    }

    private void Start()
    {
        if (LightManager.Instance != null)
        {
            LightManager.Instance.OnLightTurnDark += HandleLightTurnDark;
            LightManager.Instance.OnLightTurnBright += HandleLightTurnBright;
            LightManager.Instance.OnFlickerFinished += HandleFlickerFinished;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted += HandleNightStarted;
            GameManager.Instance.OnNightEnded += HandleNightEnded;
            GameManager.Instance.OnPlayerDead += HandlePlayerDead;
        }

        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick += TickGhost;
            NightManager.Instance.OnEnemySpawnsTick += TickSpawnCheck;
        }

        if (autoStartForTest)
        {
            BeginActivateGhost();
        }
    }

    private void OnDisable()
    {
        if (LightManager.Instance != null)
        {
            LightManager.Instance.OnLightTurnDark -= HandleLightTurnDark;
            LightManager.Instance.OnLightTurnBright -= HandleLightTurnBright;
            LightManager.Instance.OnFlickerFinished -= HandleFlickerFinished;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted -= HandleNightStarted;
            GameManager.Instance.OnNightEnded -= HandleNightEnded;
            GameManager.Instance.OnPlayerDead -= HandlePlayerDead;
        }

        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick -= TickGhost;
            NightManager.Instance.OnEnemySpawnsTick -= TickSpawnCheck;
        }
    }

    private void HandleNightStarted()
    {
        SetInactiveState();
    }

    private void HandleNightEnded()
    {
        DeactivateGhost();
    }

    private void HandlePlayerDead()
    {
        DeactivateGhost();
    }

    private void TickSpawnCheck()
    {
        if (currentState != GhostState.Inactive) return;
        if (GameManager.Instance == null || NightManager.Instance == null) return;
        if (!GameManager.Instance.IsNight) return;

        float spawnChance = GetSpawnChance();
        if (Random.value <= spawnChance)
        {
            BeginActivateGhost();
        }
    }

    public void BeginActivateGhost()
    {
        if (currentState != GhostState.Inactive) return;
        if (player == null || roomArea == null) return;
        if (LightManager.Instance == null) return;

        spawnDelayTimer = 0f;
        darkCountSinceSpawn = 0;

        currentState = GhostState.WaitingSpawn;
        SetVisible(false);
        SetColliderEnabled(false);
    }

    public void ActivateGhost()
    {
        if (currentState != GhostState.WaitingSpawn) return;
        if (player == null || roomArea == null) return;
        if (LightManager.Instance == null) return;

        int lampPower = GetLampPower();

        int flickerTotalTicks = GetFlickerTotalTicks(lampPower);
        int flickerSwitchTicks = GetFlickerSwitchTicks(lampPower);
        Vector2 blinkRange = GetBlinkDistanceRange(lampPower);

        currentBlinkStepMin = blinkRange.x;
        currentBlinkStepMax = blinkRange.y;

        transform.position = GetRandomSpawnPositionFarFromPlayer();

        currentState = GhostState.Active;
        SetVisible(true);
        SetColliderEnabled(true);

        darkCountSinceSpawn = 0;

        LightManager.Instance.SetDarkChance(darkChance);
        LightManager.Instance.SetBrightChance(brightChance);
        LightManager.Instance.StartFlicker(flickerTotalTicks, flickerSwitchTicks);
    }

    public void DeactivateGhost()
    {
        if (LightManager.Instance != null && LightManager.Instance.IsFlickering)
        {
            LightManager.Instance.StopFlicker();
        }

        SetInactiveState();
    }

    private void SetInactiveState()
    {
        currentState = GhostState.Inactive;
        spawnDelayTimer = 0f;
        darkCountSinceSpawn = 0;
        SetVisible(false);
        SetColliderEnabled(false);
    }

    private void TickGhost()
    {
        if (currentState != GhostState.WaitingSpawn) return;
        if (NightManager.Instance == null) return;

        spawnDelayTimer += NightManager.Instance.QuickInterval;

        if (spawnDelayTimer >= spawnDelaySeconds)
        {
            ActivateGhost();
        }
    }

    private void SetVisible(bool value)
    {
        if (spriteRenderer == null) return;

        Color c = spriteRenderer.color;
        c.a = value ? 1f : 0f;
        spriteRenderer.color = c;
    }

    private void SetColliderEnabled(bool value)
    {
        if (hitCollider != null)
        {
            hitCollider.enabled = value;
        }
    }

    private void HandleLightTurnDark()
    {
        if (currentState != GhostState.Active && currentState != GhostState.Warning) return;

        SetVisible(false);

        darkCountSinceSpawn++;

        if (darkCountSinceSpawn < Mathf.Max(1, blinkEveryDarkCount))
        {
            return;
        }

        darkCountSinceSpawn = 0;
        TryBlink();
    }

    private void HandleLightTurnBright()
    {
        if (currentState != GhostState.Active && currentState != GhostState.Warning) return;
        SetVisible(true);
    }

    private void HandleFlickerFinished()
    {
        DeactivateGhost();
    }

    private void TryBlink()
    {
        if (player == null) return;
        if (NightManager.Instance != null && NightManager.Instance.IsPlayerOnBed) return;

        float currentDistance = Vector2.Distance(transform.position, player.position);

        if (currentState == GhostState.Warning)
        {
            if (currentDistance <= killRange)
            {
                KillPlayer();
                return;
            }

            if (currentDistance > killRange + warningEscapeDistance)
            {
                currentState = GhostState.Active;
            }

            return;
        }

        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float blinkDistance = Random.Range(currentBlinkStepMin, currentBlinkStepMax);

        Vector2 predictedPos = (Vector2)transform.position + dirToPlayer * blinkDistance;
        float predictedDistance = Vector2.Distance(predictedPos, player.position);

        if (predictedDistance <= killRange)
        {
            Vector2 warningPos = (Vector2)player.position - dirToPlayer * warningStopDistance;
            Vector3 warningTarget = new Vector3(warningPos.x, warningPos.y, transform.position.z);

            transform.position = GetValidPointInRoom(transform.position, warningTarget);
            currentState = GhostState.Warning;
            return;
        }

        Vector3 predictedTarget = new Vector3(predictedPos.x, predictedPos.y, transform.position.z);
        transform.position = GetValidPointInRoom(transform.position, predictedTarget);
    }

    private void KillPlayer()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDead("Killed by Blink Ghost");
        }

        DeactivateGhost();
    }

    private int GetLampPower()
    {
        if (lightFacility == null) return 0;
        return lightFacility.AllocatedPower;
    }

    private float GetSpawnChance()
    {
        float chance = baseSpawnChance;

        if (GameManager.Instance != null)
        {
            chance += GameManager.Instance.CurrentDay * spawnChancePerDay;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            chance += progress * spawnChanceByNightProgress;
        }

        chance -= GetLampPower() * spawnChanceReducePerPower;

        return Mathf.Clamp01(chance);
    }

    private int GetFlickerTotalTicks(int lampPower)
    {
        int result = baseFlickerTotalTicks;

        if (GameManager.Instance != null)
        {
            result += GameManager.Instance.CurrentDay * flickerTicksPerDay;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            result += Mathf.RoundToInt(progress * flickerTicksByNightProgress);
        }

        result -= lampPower * flickerTicksReducePerPower;

        return Mathf.Clamp(result, 10, 36);
    }

    private int GetFlickerSwitchTicks(int lampPower)
    {
        int result = baseFlickerSwitchTicks;

        if (GameManager.Instance != null)
        {
            result -= (GameManager.Instance.CurrentDay / 2) * switchTicksReducePer2Day;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            result -= Mathf.RoundToInt(progress * switchTicksReduceByNightProgress);
        }

        result += lampPower * switchTicksAddPerPower;

        return Mathf.Clamp(result, 1, 3);
    }

    private Vector2 GetBlinkDistanceRange(int lampPower)
    {
        float min = baseBlinkStepMin;
        float max = baseBlinkStepMax;

        if (GameManager.Instance != null)
        {
            float dayBonus = GameManager.Instance.CurrentDay * blinkDistancePerDay;
            min += dayBonus;
            max += dayBonus;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            float timeBonus = progress * blinkDistanceByNightProgress;
            min += timeBonus;
            max += timeBonus;
        }

        float powerReduce = lampPower * blinkDistanceReducePerPower;
        min -= powerReduce;
        max -= powerReduce;

        min = Mathf.Clamp(min, 0.8f, 2.2f);
        max = Mathf.Clamp(max, min + 0.2f, 3.2f);

        return new Vector2(min, max);
    }

    private Vector3 GetRandomSpawnPositionFarFromPlayer()
    {
        if (roomArea == null)
        {
            return transform.position;
        }

        Bounds bounds = roomArea.bounds;

        for (int i = 0; i < spawnTryCount; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );

            if (!roomArea.OverlapPoint(candidate))
                continue;

            if (Vector2.Distance(candidate, player.position) < minSpawnDistanceFromPlayer)
                continue;

            return candidate;
        }

        return transform.position;
    }

    private Vector3 GetValidPointInRoom(Vector3 start, Vector3 target)
    {
        if (roomArea == null)
            return target;

        if (roomArea.OverlapPoint(target))
            return target;

        const int sampleCount = 20;

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector3 point = Vector3.Lerp(target, start, t);

            if (roomArea.OverlapPoint(point))
            {
                point.z = transform.position.z;
                return point;
            }
        }

        return start;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (roomArea != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(roomArea.bounds.center, roomArea.bounds.size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningStopDistance);
    }
#endif
}