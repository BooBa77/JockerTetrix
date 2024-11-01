using UnityEngine;
using UnityEngine.UI;

public class Level_00 : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    private Vector2 startPosition; // Начальная позиция Джокера
    public Vector2 targetPosition = new Vector2(-96, -175); // Позиция говорящего Джокера
    private float durationEnter = 0.2f; // Время выхода Джокера
    private float durationText = 1f; // Время отображения текста
    private float durationFly = 0.5f; // Время полета вверх
    private float startTime; // Время начала анимации

    void Start()
    {
        // Начальное положение Image Джокера
        startPosition = jocker.rectTransform.anchoredPosition;
        startTime = Time.time; // Время начала анимации
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationFly) // Если вышло время выхода, речи и улёта
        {
            Destroy(gameObject); // Уничтожаем Canvas, но котором этот скрипт
        }
        else if (Time.time - startTime >= durationEnter + durationText) // Если вышло время выхода и речи
        {
            // Джокер поговорил и улетает
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationFly;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, Vector2.up * Screen.height, progress); // Позиция
                jocker.rectTransform.localScale = Vector3.Lerp(jocker.rectTransform.localScale, new Vector3(0.3f, 0.3f, 1f), progress); // Масштаб
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
