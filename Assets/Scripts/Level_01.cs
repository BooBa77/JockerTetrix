using UnityEngine;
using UnityEngine.UI;

public class Level_01 : MonoBehaviour
{
    [SerializeField] Image jocker1;
    [SerializeField] Image jocker2;
    [SerializeField] Image jockerText;
    private Vector2 startPosition1; // Начальная позиция Джокера инженера
    private Vector2 startPosition2; // Начальная позиция Джокера строителя
    public Vector2 targetPosition1 = new Vector2(-240, -207); // Позиция Джокера инженера
    public Vector2 targetPosition2 = new Vector2(223, -202); // Позиция Джокера строителя
    private float durationEnter = 0.2f; // Время на появление Джокеров
    private float durationText = 2f; // Время отображения текста
    private float durationExit = 0.5f; // Время на уход Джокеров
    private float startTime; // Время начала анимации

    void Start()
    {
        // Начальное положение Image Джокеров
        startPosition1 = jocker1.rectTransform.anchoredPosition;
        startPosition2 = jocker2.rectTransform.anchoredPosition;
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
                jocker1.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition1, startPosition1, progress);
                jocker2.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition2, startPosition2, progress);
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
                jocker1.rectTransform.anchoredPosition = Vector2.Lerp(startPosition1, targetPosition1, progress);
                jocker2.rectTransform.anchoredPosition = Vector2.Lerp(startPosition2, targetPosition2, progress);
            }
        }
    }
}
