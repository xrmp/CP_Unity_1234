using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int objectCount = 500;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float rotationSpeed = 50f;

    private TransformAccessArray transformAccessArray;
    private NativeArray<Vector3> startPositions;
    private NativeArray<float> angles;
    private JobHandle jobHandle;
    private bool isJobRunning = false;

    private void Start()
    {
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        // Инициализация массивов
        startPositions = new NativeArray<Vector3>(objectCount, Allocator.Persistent);
        angles = new NativeArray<float>(objectCount, Allocator.Persistent);

        Transform[] transforms = new Transform[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            // Случайная позиция по кругу
            float angle = Random.Range(0f, 360f);
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
            float z = Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;
            Vector3 position = new Vector3(x, 0, z);

            // Спавн объекта
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            transforms[i] = obj.transform;

            // Сохраняем данные
            startPositions[i] = position;
            angles[i] = angle;
        }

        transformAccessArray = new TransformAccessArray(transforms);
        isJobRunning = true;
    }

    private void Update()
    {
        if (!isJobRunning) return;

        // Завершаем предыдущий job
        jobHandle.Complete();

        // Создаем и запускаем новый job
        CircleMovementJob job = new CircleMovementJob
        {
            StartPositions = startPositions,
            Angles = angles,
            DeltaTime = Time.deltaTime,
            RotationSpeed = rotationSpeed,
            TimeValue = Time.time
        };

        jobHandle = job.Schedule(transformAccessArray);

        // Планируем завершение в следующем кадре
        JobHandle.ScheduleBatchedJobs();
    }

    private void LateUpdate()
    {
        // Завершаем job перед использованием transforms
        jobHandle.Complete();
    }

    private void OnDestroy()
    {
        // Очистка ресурсов
        jobHandle.Complete();
        if (transformAccessArray.isCreated)
            transformAccessArray.Dispose();
        if (startPositions.IsCreated)
            startPositions.Dispose();
        if (angles.IsCreated)
            angles.Dispose();
    }
    public int GetObjectCount() => objectCount;
}

[BurstCompile]
public struct CircleMovementJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<Vector3> StartPositions;
    public NativeArray<float> Angles;
    public float DeltaTime;
    public float RotationSpeed;
    public float TimeValue;

    public void Execute(int index, TransformAccess transform)
    {
        // Обновляем угол
        Angles[index] += RotationSpeed * DeltaTime * 10;
        if (Angles[index] > 360) Angles[index] -= 360;

        // Вычисляем новую позицию по кругу
        float rad = Angles[index] * Mathf.Deg2Rad;
        Vector3 startPos = StartPositions[index];

        // Создаем круговое движение
        float x = Mathf.Sin(rad) * startPos.magnitude;
        float z = Mathf.Cos(rad) * startPos.magnitude;

        transform.position = new Vector3(x, Mathf.Sin(TimeValue + index) * 2, z);
        transform.rotation = Quaternion.Euler(0, Angles[index], 0);
    }
}