using UnityEngine;

public enum ShadowHandState
{
    Approaching,   // 正在逼近目标
    Recoiling,     // 被踩住时后退
    Dead
}

public class ShadowHandEnemy : MonoBehaviour
{
    [Header("组件")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;

    [Header("两帧动画")]
    [SerializeField] private Sprite frameA;
    [SerializeField] private Sprite frameB;
    [SerializeField] private int normalAnimTickInterval = 2;   // 平时每几个QuickTick切一次帧
    [SerializeField] private int steppedAnimTickInterval = 1;  // 被踩住时动画加速

    [Header("移动参数")]
    [SerializeField] private float moveStep = 0.12f;      // 每次QuickTick前进距离
    [SerializeField] private float recoilStep = 0.18f;    // 被踩住时后退距离（比前进稍快）
    [SerializeField] private float hitDistance = 0.08f;   // 认为碰到设施的距离

    [Header("踩压参数")]
    [SerializeField] private int stompRequiredTicks = 8;  // 需要持续踩住多少个QuickTick才会消失
    [SerializeField] private int stompDecayPerTick = 1;   // 没踩住时每Tick衰减多少

    [Header("消失淡出")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private int fadeTicks = 4;

    [Header("附身参数")]
    [SerializeField] private int basePossessTicks = 6;              // 基础附身tick（按EnemySpawnsTick算）
    [SerializeField] private int possessTicksPerDay = 2;            // 每天增加多少tick
    [SerializeField] private float possessTicksByNightProgress = 4f;// 夜晚进度最多额外加多少tick
    [SerializeField] private float allocatedPowerResist = 1f;       // 每点电力抵消多少tick

    private Facilitybase targetFacility;
    private Vector3 spawnPosition;
    private ShadowHandState currentState = ShadowHandState.Dead;

    private bool useFrameA = true;
    private int animTickCounter = 0;

    private int playerContactCount = 0;
    private int currentStompTicks = 0;

    private bool isFadingOut = false;
    private int fadeRemainTicks = 0;
    private int fadeTotalTicks = 0;
    private Color originalColor;

    public Facilitybase TargetFacility => targetFacility;
    public bool IsPlayerStepping
    {
        get
        {
            if (NightManager.Instance != null && NightManager.Instance.IsPlayerOnBed)
            {
                return false;
            }
            return playerContactCount > 0;
        }
    }
    public bool IsAlive => currentState != ShadowHandState.Dead;

    public void Init(Facilitybase target)
    {
        targetFacility = target;
        spawnPosition = transform.position;
        currentState = ShadowHandState.Approaching;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.sprite = frameA;
        }

        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick += TickQuick;
        }
    }

    private void TickQuick()
    {
        if (currentState == ShadowHandState.Dead)
            return;

        TickAnimation();

        if (isFadingOut)
        {
            TickFadeToBlack();
            return;
        }

        switch (currentState)
        {
            case ShadowHandState.Approaching:
                TickApproach();
                break;

            case ShadowHandState.Recoiling:
                TickRecoil();
                break;
        }
    }

    private void TickAnimation()
    {
        if (spriteRenderer == null || frameA == null || frameB == null)
            return;

        int interval = IsPlayerStepping ? steppedAnimTickInterval : normalAnimTickInterval;
        interval = Mathf.Max(1, interval);

        animTickCounter++;
        if (animTickCounter < interval)
            return;

        animTickCounter = 0;
        useFrameA = !useFrameA;
        spriteRenderer.sprite = useFrameA ? frameA : frameB;
    }

    private void TickApproach()
    {
        if (targetFacility == null)
        {
            SelfDestroy();
            return;
        }

        if (IsPlayerStepping)
        {
            currentState = ShadowHandState.Recoiling;
            TickRecoil();
            return;
        }

        HandleStompDecay();

        Vector3 targetPos = targetFacility.transform.position;
        RotateToward(targetPos);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);

        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= hitDistance)
        {
            HitTarget();
        }
    }

    private void TickRecoil()
    {

        if (!IsPlayerStepping)
        {
            currentState = ShadowHandState.Approaching;
            return;
        }

        currentStompTicks++;

        Vector3 recoilTarget = spawnPosition;
        transform.position = Vector3.MoveTowards(transform.position, recoilTarget, recoilStep);

        if (currentStompTicks >= stompRequiredTicks)
        {
            BeginFadeOut();
            return;
        }
    }

    private void HandleStompDecay()
    {
        currentStompTicks -= stompDecayPerTick;
        if (currentStompTicks < 0)
        {
            currentStompTicks = 0;
        }
    }

    private void RotateToward(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 美术素材不正，所以减77度
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 77f);
    }

    private void HitTarget()
    {
        if (targetFacility == null)
        {
            SelfDestroy();
            return;
        }

        int possessTicks = GetPossessTicks(targetFacility);
        targetFacility.GetPossessed(possessTicks);

        BeginFadeOut();
    }

    private int GetPossessTicks(Facilitybase target)
    {
        int result = basePossessTicks;

        if (GameManager.Instance != null)
        {
            result += GameManager.Instance.CurrentDay * possessTicksPerDay;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            result += Mathf.RoundToInt(progress * possessTicksByNightProgress);
        }

        // 电力分配越高，抵抗越强，附身时间越短
        result -= Mathf.RoundToInt(target.AllocatedPower * allocatedPowerResist);

        return Mathf.Max(3, result);
    }

    private void BeginFadeOut()
    {
        isFadingOut = true;
        fadeTotalTicks = Mathf.Max(1, fadeTicks);
        fadeRemainTicks = fadeTotalTicks;
    }

    private void TickFadeToBlack()
    {
        if (spriteRenderer == null)
        {
            SelfDestroy();
            return;
        }

        if (fadeRemainTicks <= 0)
        {
            SelfDestroy();
            return;
        }

        fadeRemainTicks--;

        int elapsed = fadeTotalTicks - fadeRemainTicks;
        float t = Mathf.Clamp01((float)elapsed / fadeTotalTicks);
        spriteRenderer.color = Color.Lerp(originalColor, fadeColor, t);

        if (fadeRemainTicks <= 0)
        {
            SelfDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerContactCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerContactCount--;
            if (playerContactCount < 0)
            {
                playerContactCount = 0;
            }
        }
    }

    private void SelfDestroy()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick -= TickQuick;
        }

        currentState = ShadowHandState.Dead;
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick -= TickQuick;
        }
    }
}