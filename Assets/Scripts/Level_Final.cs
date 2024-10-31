using UnityEngine;
using UnityEngine.UI;

public class Level_Final : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    public Vector2 startPosition; // Начальная позиция Джокера горлума
    public Vector2 targetPosition; // Основная позиция Джокера горлума
    public Vector2 endPosition; // Позиция Джокера горлума в конце анимации
    private float durationEnter = 0.2f; // Время на появление Джокера
    private float durationText = 2f; // Время отображения текста
    private float durationExit = 0.5f; // Время на уход Джокера
    private float startTime; // Время начала анимации

    void Start()
    {
        // Начальное положение Image Джокера
        startPosition = jocker.rectTransform.anchoredPosition;
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
            // Джокер поговорил и уходит в сторону
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, endPosition, progress);
                jocker.rectTransform.localScale = Vector3.Lerp(jocker.rectTransform.localScale, new Vector3(1f, 1f, 1f), progress); // Масштаб
            }
        }
        else if (Time.time - startTime >= durationEnter) // Если вышло время выхода
        {
            // Джокер говорит 2 секунды
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
