using Unity.Mathematics; // ✅ ВАЖНО: для math.log()
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;
using System.Text;

public class SpawnManagerExtended : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int objectCount = 500;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Logarithm Settings")]
    [SerializeField] private float logCalculationInterval = 2f;
    [SerializeField] private bool showLogResults = true;

    private TransformAccessArray transformAccessArray;
    private NativeArray<Vector3> startPositions;
    private NativeArray<float> angles;
    private NativeArray<float> logResults;

    private JobHandle movementJobHandle;
    private JobHandle logJobHandle;
    private bool isJobRunning = false;

    private float nextLogCalculationTime;
    private StringBuilder logStringBuilder;

    private void Start()
    {
        SpawnObjects();
        nextLogCalculationTime = Time.time + logCalculationInterval;
        logStringBuilder = new StringBuilder();
    }

    private void SpawnObjects()
    {
        startPositions = new NativeArray<Vector3>(objectCount, Allocator.Persistent);
        angles = new NativeArray<float>(objectCount, Allocator.Persistent);
        logResults = new NativeArray<float>(objectCount, Allocator.Persistent);

        Transform[] transforms = new Transform[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
            float z = Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;
            Vector3 position = new Vector3(x, 0, z);

            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            transforms[i] = obj.transform;

            startPositions[i] = position;
            angles[i] = angle;
            logResults[i] = 0f;
        }

        transformAccessArray = new TransformAccessArray(transforms);
        isJobRunning = true;
    }

    private void Update()
    {
        if (!isJobRunning) return;

        movementJobHandle.Complete();
        logJobHandle.Complete();

        CircleMovementJobV2 movementJob = new CircleMovementJobV2
        {
            StartPositions = startPositions,
            Angles = angles,
            DeltaTime = Time.deltaTime,
            RotationSpeed = rotationSpeed,
            TimeValue = Time.time
        };

        movementJobHandle = movementJob.Schedule(transformAccessArray);

        if (Time.time >= nextLogCalculationTime)
        {
            movementJobHandle.Complete();

            LogarithmJob logJob = new LogarithmJob
            {
                LogResults = logResults,
                Seed = (uint)(Time.time * 1000)
            };

            logJobHandle = logJob.Schedule(objectCount, 64);
            nextLogCalculationTime = Time.time + logCalculationInterval;

            if (showLogResults)
                ScheduleLogOutput();
        }

        JobHandle.ScheduleBatchedJobs();
    }

    private void ScheduleLogOutput()
    {
        logJobHandle.Complete();

        logStringBuilder.Clear();
        logStringBuilder.AppendLine($"<b>=== LOGARITHM CALCULATIONS ({Time.time:F1}s) ===</b>");

        int displayCount = Mathf.Min(10, objectCount);
        float sum = 0f;

        for (int i = 0; i < displayCount; i++)
        {
            logStringBuilder.AppendLine($"Object {i}: Log = {logResults[i]:F4}");
            sum += logResults[i];
        }

        logStringBuilder.AppendLine($"Average log (sample): {sum / displayCount:F4}");
        logStringBuilder.AppendLine($"<b>========================================</b>");

        Debug.Log(logStringBuilder.ToString());
    }

    private void LateUpdate()
    {
        movementJobHandle.Complete();
    }

    private void OnDestroy()
    {
        movementJobHandle.Complete();
        logJobHandle.Complete();

        if (transformAccessArray.isCreated)
            transformAccessArray.Dispose();
        if (startPositions.IsCreated)
            startPositions.Dispose();
        if (angles.IsCreated)
            angles.Dispose();
        if (logResults.IsCreated)
            logResults.Dispose();
    }

    // ✅ ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ PERFORMANCE DISPLAY
    public int GetObjectCount() => objectCount;
    public float GetNextCalcTime() => nextLogCalculationTime;
}

[BurstCompile]
public struct CircleMovementJobV2 : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<Vector3> StartPositions;
    public NativeArray<float> Angles;
    public float DeltaTime;
    public float RotationSpeed;
    public float TimeValue;

    public void Execute(int index, TransformAccess transform)
    {
        Angles[index] += RotationSpeed * DeltaTime * 10;
        if (Angles[index] > 360) Angles[index] -= 360;

        float rad = Angles[index] * Mathf.Deg2Rad;
        Vector3 startPos = StartPositions[index];

        float x = Mathf.Sin(rad) * startPos.magnitude;
        float z = Mathf.Cos(rad) * startPos.magnitude;

        transform.position = new Vector3(x, Mathf.Sin(TimeValue + index) * 2, z);
        transform.rotation = Quaternion.Euler(0, Angles[index], 0);
    }
}

[BurstCompile]
public struct LogarithmJob : IJobParallelFor
{
    public NativeArray<float> LogResults;
    public uint Seed;

    public void Execute(int index)
    {
        uint randomState = Seed + (uint)index * 0x9e3779b9;
        randomState = (randomState ^ 61) ^ (randomState >> 16);
        randomState *= 9;
        randomState ^= randomState >> 4;
        randomState *= 0x27d4eb2d;
        randomState ^= randomState >> 15;

        float randomFloat = (randomState & 0xFFFFFF) / 16777216.0f;
        float number = 1 + randomFloat * 99;

        // ✅ ИСПРАВЛЕНО: работает с using Unity.Mathematics;
        LogResults[index] = math.log(number);
    }
}