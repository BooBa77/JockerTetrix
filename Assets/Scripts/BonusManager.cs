using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BonusManager : MonoBehaviour
{
    public int defaultTimerBatman; // Сколько в среднем ожидать знак Бэтмэна
    [SerializeField] LevelManager levelManager; // ссылка на скрипт, управления уровнями игры
    [SerializeField] Main main; // Ссылка на основной скрипт игры
    public GameObject[] bonusLeft; // Массив кнопок бонусов левого
    public GameObject[] bonusRight; // Массив кнопок бонусов правого
    public Sprite[] bonusSprite; // Массив иконок бонусов
    public int bonusCountLeft; // Количество бонусов собранных левым игроком
    public int bonusCountRight; // Количество бонусов собранных правым игроком

    [SerializeField] Projector projectorLeftScript; // Ссылка на скрипт левого прожектора
    private float projectorLeftTimer; // Таймер левого прожектора
    public Vector2 projectorLeftPosition; // Координаты цели левого прожектора, если (0,0) значит выключен
    [SerializeField] Projector projectorRightScript; // Ссылка на скрипт правого прожектора
    public float projectorRightTimer; // Таймер правого прожектора
    public Vector2 projectorRightPosition; // Координаты цели правого прожектора, если (0,0) значит выключен

    void Start()
    {
        projectorLeftTimer = NewTimer(defaultTimerBatman); projectorLeftScript.waitTimer = false;
        projectorRightTimer = NewTimer(defaultTimerBatman); projectorRightScript.waitTimer = false;

    }

    void Update()
    {
        if (Main.stop_game) { return; } // Если игра остановлена, отдыхаем

        // Нажатие кнопок 
        if (Input.GetKeyDown(KeyCode.Q) && bonusCountLeft > 0) { UseBonus(0, 0); } // Использование 1 левого бонуса


        if (projectorLeftScript.targetPosition.x == 0f) // Левый прожектор пока не включен
        {
            projectorLeftTimer -= Time.deltaTime; // Приближаем вызов Бэтмена
            if (projectorLeftTimer < 0) // Слева пришло время вызвать Бэтмена
            {
                if (projectorLeftScript.waitTimer == true)
                {
                    projectorLeftPosition = new Vector2(0f, -10f);
                    projectorLeftTimer = NewTimer(defaultTimerBatman); // Назначаем новый таймер
                    projectorLeftScript.waitTimer = false;
                }
                else
                {
                    Vector2 placeSign = FindPlaceForSign(); // Определяем освещаяемую ячейку
                    if (placeSign.x == 0) { return; } // Нет подходящих ячеек
                    projectorLeftScript.targetPosition = placeSign; // Налевляем левый прожектор на выбранную ячейку
                    projectorLeftScript.EnableProjector(); // Влючаем левый прожектор
                    projectorLeftPosition = placeSign;
                }
            }
        }

        if (projectorRightScript.targetPosition.x == 0f) // Правый прожектор пока не включен
        { 
            projectorRightTimer -= Time.deltaTime; // Приближаем вызов Бэтмена
            if (projectorRightTimer < 0) // Справа пришло время вызвать Бэтмена
            {
                if (projectorRightScript.waitTimer == true)
                {
                    projectorRightPosition = new Vector2(0f, -10f);
                    projectorRightTimer = NewTimer(defaultTimerBatman); // Назначаем новый таймер
                    projectorRightScript.waitTimer = false;
                }
                else
                {
                    Vector2 placeSign = FindPlaceForSign(); // Определяем освещаяемую ячейку
                    if (placeSign.x == 0) { return; } // Нет подходящих ячеек
                    projectorRightScript.targetPosition = placeSign; // Направляем правый прожектор на выбранную ячейку
                    projectorRightScript.EnableProjector(); // Влючаем правый прожектор
                    projectorRightPosition = placeSign;
                }
            }
        }
    }
    private int NewTimer(int defaultTimer) // Задаёт таймер включения прожектора
    {
        return defaultTimer; // Пока по-умолчанию
    }

    private Vector2 FindPlaceForSign() // Поиск освещаемой прожектором ячейки
    {
        Dictionary<int, Vector2> signPositions = new Dictionary<int, Vector2>(); // Словарь подходящих ячеек
        for (int y = 1; y < 40; y++)
        {
            for (int x = 1; x < 15; x++)
            {
                if (main.space_cells_status[x, y] == "classic" // Целями для прожекторов могут быть только ячейки типа "classic"
                    && (x != projectorLeftPosition.x && y != projectorLeftPosition.y) // И не занятые левым
                    && (x != projectorRightPosition.x && y != projectorRightPosition.y)) // Или правым прожектором
                {
                    signPositions.Add(signPositions.Count, new Vector2(x, y)); // Добавляем в словарь координаты возможной позиции для знака
                }
            }
        }
        if (signPositions.Count > 0) // Если подходящие ячейки найдены
        {
            int randomIndex = Random.Range(0, signPositions.Count); // Выбираем случайный индекс из списка
            return signPositions[randomIndex]; // Возвращаем координаты освещаемой ячейки
        }
        return (new Vector2(0, 0)); // Ячейка не найдена
    }

    public void TakeBonus(Vector2 bonusPosition, int N_shape)
    {
        if (bonusPosition == projectorLeftPosition)
        {
            projectorLeftScript.DisableGameObject();
            projectorLeftScript.ReplaceSign(new Vector2(0f, -10f)); // Чтобы не видеть отложенные мерцания прожектора
            projectorLeftPosition = new Vector2(0f, -10f); // Выключаем прожектор
            projectorLeftTimer = NewTimer(defaultTimerBatman); // Назначаем новый таймер
        }
        if (bonusPosition == projectorRightPosition)
        {
            projectorRightScript.DisableGameObject();
            projectorRightScript.ReplaceSign(new Vector2(0f, -10f)); // Чтобы не видеть отложенные мерцания прожектора
            projectorRightPosition = new Vector2(0f, -10f); // Выключаем прожектор
            projectorRightTimer = NewTimer(defaultTimerBatman); // Назначаем новый таймер
        }
        if (N_shape == 0) 
        { 
            if (bonusCountLeft < 6)
            {
                while (true)
                {
                    int randomNumber = Random.Range(0, 6);
                    if (bonusLeft[randomNumber].GetComponent<Image>().sprite = bonusSprite[0])
                    {
                        bonusLeft[randomNumber].GetComponent<Image>().sprite = bonusSprite[1];
                        break;
                    }
                }
                bonusCountLeft++;
            }
            

        }
        else if (N_shape == 1) { bonusCountRight++; }
    }

    public void ReplaceSign(Vector2 oldPosition, Vector2 newPosition) // Перенос знака из-за смещения ячейки
    {
        if (oldPosition == projectorLeftPosition)
        {
            projectorLeftScript.ReplaceSign(newPosition);
            projectorLeftPosition = newPosition;
        }
        else
        { 
            projectorRightScript.ReplaceSign(newPosition);
            projectorRightPosition = newPosition;
        }
    }

    public void UseBonus(int side, int N_bonus)
    {
        return;
    }
}
