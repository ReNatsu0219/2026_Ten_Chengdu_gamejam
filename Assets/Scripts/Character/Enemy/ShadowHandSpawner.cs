using System.Collections.Generic;
using UnityEngine;

public class ShadowHandSpawner : MonoBehaviour
{
    [Header("预制体")]
    [SerializeField] private ShadowHandEnemy shadowHandPrefab;

    [Header("生成点（放在地图四周/屏幕边缘外）")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("可作为目标的设施")]
    [SerializeField] private Facilitybase[] targetFacilities;

    [Header("地图中心参考点")]
    [SerializeField] private Transform mapCenterPoint;

    [Header("中央约束矩形区域")]
    [SerializeField] private Vector2 centerRegionSize = new Vector2(4f, 3f);

    [Header("生成参数")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private int maxHandCount = 3;
    [SerializeField] private float baseSpawnRate = 0.01f;
    [SerializeField] private float spawnRatePerDay = 0.02f;
    [SerializeField] private float spawnRateByNightProgress = 0.2f;

    private readonly List<ShadowHandEnemy> activeHands = new List<ShadowHandEnemy>();

    private void Start()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnEnemySpawnsTick += TrySpawnHand;
        }
    }

    private void OnDisable()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnEnemySpawnsTick -= TrySpawnHand;
        }
    }

    private void TrySpawnHand()
    {
        if (!isActive) return;
        if (shadowHandPrefab == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (targetFacilities == null || targetFacilities.Length == 0) return;

        CleanupNullHands();

        if (activeHands.Count >= maxHandCount) return;

        float finalRate = GetCurrentSpawnRate();
        if (Random.value > finalRate) return;

        Facilitybase target = GetRandomValidTarget();
        if (target == null) return;

        Transform spawnPoint = GetSpawnPointForTarget(target);
        if (spawnPoint == null) return;

        ShadowHandEnemy newHand = Instantiate(shadowHandPrefab, spawnPoint.position, Quaternion.identity);
        newHand.Init(target);
        activeHands.Add(newHand);
    }

    private float GetCurrentSpawnRate()
    {
        float rate = baseSpawnRate;

        if (GameManager.Instance != null)
        {
            rate += GameManager.Instance.CurrentDay * spawnRatePerDay;
        }

        if (NightManager.Instance != null && NightManager.Instance.NightDuration > 0f)
        {
            float progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
            rate += progress * spawnRateByNightProgress;
        }

        return Mathf.Clamp01(rate);
    }

    private Facilitybase GetRandomValidTarget()
    {
        List<Facilitybase> validTargets = new List<Facilitybase>();

        foreach (Facilitybase facility in targetFacilities)
        {
            if (facility == null) continue;
            if (!facility.gameObject.activeInHierarchy) continue;

            validTargets.Add(facility);
        }

        Debug.Log("Valid SpawnPoints Count: " + validTargets.Count);

        if (validTargets.Count == 0) return null;

        return validTargets[Random.Range(0, validTargets.Count)];
    }

    private Transform GetSpawnPointForTarget(Facilitybase target)
    {
        if (target == null) return null;

        List<Transform> validSpawnPoints = new List<Transform>();
        Vector2 targetPos = target.transform.position;

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            Vector2 spawnPos = point.position;

            if (!IsSpawnPointOnFarSide(spawnPos, targetPos))
                continue;

            if (!DoesPathCrossCenterRegion(spawnPos, targetPos))
                continue;

            validSpawnPoints.Add(point);
        }

        if (validSpawnPoints.Count == 0)
        {
            return null;
        }

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

    private bool IsSpawnPointOnFarSide(Vector2 spawnPos, Vector2 targetPos)
    {
        Vector2 mapCenter = GetMapCenter();

        Vector2 targetDir = targetPos - mapCenter;
        Vector2 spawnDir = spawnPos - mapCenter;

        // 在目标相对地图中心的反侧
        return Vector2.Dot(spawnDir, targetDir) < 0f;
    }

    private bool DoesPathCrossCenterRegion(Vector2 start, Vector2 end)
    {
        Vector2 regionCenter = GetMapCenter();
        Vector2 halfSize = centerRegionSize * 0.5f;

        const int sampleCount = 24;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector2 point = Vector2.Lerp(start, end, t);

            bool insideX = point.x >= regionCenter.x - halfSize.x && point.x <= regionCenter.x + halfSize.x;
            bool insideY = point.y >= regionCenter.y - halfSize.y && point.y <= regionCenter.y + halfSize.y;

            if (insideX && insideY)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetMapCenter()
    {
        if (mapCenterPoint != null)
        {
            return mapCenterPoint.position;
        }

        return transform.position;
    }

    private void CleanupNullHands()
    {
        activeHands.RemoveAll(hand => hand == null);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 center = GetMapCenter();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, centerRegionSize);

        if (spawnPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;
                Gizmos.DrawSphere(point.position, 0.12f);
            }
        }
    }
#endif
}