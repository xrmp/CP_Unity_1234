using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class CounterDisplay : MonoBehaviour
{
    [SerializeField] private Text counterText;
    private EntityManager entityManager;
    private EntityQuery counterQuery;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        counterQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<CounterComponent>()
        );

        if (counterText == null)
            CreateCounterDisplay();
    }

    private void CreateCounterDisplay()
    {
        var canvasGO = new GameObject("CounterCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var textGO = new GameObject("CounterText");
        textGO.transform.SetParent(canvasGO.transform);

        counterText = textGO.AddComponent<Text>();
        counterText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        counterText.fontSize = 32;
        counterText.color = Color.white;
        counterText.alignment = TextAnchor.UpperLeft;

        var rect = counterText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(300, 100);
    }

    private void Update()
    {
        if (counterQuery.IsEmptyIgnoreFilter)
            return;

        var counter = counterQuery.GetSingleton<CounterComponent>();
        counterText.text = $"<b>ECS COUNTER</b>\nValue: {counter.Value}\nFPS: {(1.0f / Time.deltaTime):F1}";
    }
}