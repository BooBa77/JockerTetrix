using UnityEngine;
using UnityEngine.UI;

public class Level_Gandalf : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    public Vector2 startPosition; // Начальная позиция Джокера
    public Vector2 targetPosition; // Позиция Джокера
    private float durationEnter = 0.2f; // Время на появление Джокера
    private float durationText = 2f; // Время отображения текста
    private float durationExit = 0.5f; // Время на уход Джокера
    private float startTime; // Время начала анимации

    void Start()
    {
        // Начальное положение Image Джокеров
        startTime = Time.time; // Время начала анимации
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationExit) // Если вышло время входа, речи и ухода
        {
            Destroy(gameObject); // Уничтожаем Canvas, но котором этот скрипт
        }
        else if (Time.time - startTime >= durationEnter + durationText) // Если вышло время выхода и речи
        {
            // Джокер поговорил и уходит
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, startPosition, progress);
            }
        }
        else if (Time.time - startTime >= durationEnter) // Если вышло время выхода
        {
            // Джокер говорит
            jockerText.enabled = true;
        }
        else
        {
            // Анимируем движение выхода Джокера на экран
            float progress = (Time.time - startTime) / durationEnter;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            }
        }
    }
}
