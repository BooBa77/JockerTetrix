using UnityEngine;
using UnityEngine.UI;

public class Level_02 : MonoBehaviour
{
    [SerializeField] Image Batman; // Картинка Бэтмэна в лицо
    [SerializeField] Sprite batmanBack; // спрайт Бэтмэна со спины
    [SerializeField] Image Bomb1; // Картинки бомб
    [SerializeField] Image Bomb2;
    private Vector2 startPosition; // Начальная позиция Бэтмена
    public Vector2 targetPosition = new Vector2(66, -148); // Основная позиция Бэтмэна
    public Vector2 finishPosition = new Vector2(56, 508); // Позиция Бэтмена в конце анимэ
    private float durationEnter = 0.2f; // Время на появление Бэтмэна
    private float durationText = 2f; // Время отображения
    private float durationExit = 0.5f; // Время на уход Бэтмэна
    private float startTime; // Время начала анимации

    void Start()
    {
        // Начальное положение Image Бэтмэна
        startPosition = Batman.rectTransform.anchoredPosition;
        startTime = Time.time; // Время начала анимации
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationExit) // Если вышло время входа, отображения и ухода
        {
            Destroy(gameObject); // Уничтожаем Canvas, но котором этот скрипт
        }
        else if (Time.time - startTime >= durationEnter + durationText) // Если вышло время выхода и отображения
        {
            // Бэтмэн повисел и уходит
            Bomb1.enabled = false; Bomb2.enabled = false;
            Batman.sprite = batmanBack;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                Batman.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, finishPosition, progress);
            }
        }
        else if (Time.time - startTime >= durationEnter) // Если вышло время выхода
        {
            // Batman достал бомбы и висит
            Bomb1.enabled = true; Bomb2.enabled = true;
        }
        else
        {
            // Анимируем движение выхода Бэтмэна на экран
            float progress = (Time.time - startTime) / durationEnter;
            if (progress <= 1)
            {
                Batman.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            }
        }
    }
}
