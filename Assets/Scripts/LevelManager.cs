using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Main main; // Ссылка на основной скрипт игры
    public int level = 0; // Текущий уровень игры
    private int levels_count; // Количество уровней в игре
    public float level_progress; // Прогресс прохождения текущего уровня
    public Text Result; // Результат игры, выводится в окне поражения
    public int shapes_count = 0; // Счётчик упавших за уровень фигур
    public int vip_shape = -1; // Номер ключевой фигуры блока. Используется, чтобы определить упала ли она
    [SerializeField] GameObject startWindow; // Канвас стартовый
    [SerializeField] GameObject victoryWindow; // Канвас победный
    [SerializeField] GameObject defeatWindow; // Канвас поражения
    [SerializeField] GameObject game_interface; // Канвас с джойстиками
    // Звуки
    public AudioSource sound_background; // Фоновая музыка
    [SerializeField] AudioSource sound_victory; // Победа
    [SerializeField] AudioSource sound_defeat; // Поражение

    // Джокеры
    [SerializeField] GameObject level00_anime; // Префаб с анимацией 0-го уровня
    [SerializeField] GameObject level01_anime; // Префаб с анимацией 1-го уровня
    [SerializeField] GameObject level02_anime; // Префаб с анимацией 2-го уровня
    [SerializeField] GameObject levelGandalf_anime; // Префаб с анимацией Гулливер уровней
    [SerializeField] GameObject levelFinal_anime; // Префаб с анимацией финального уровня


    public void SetNewShape()
    {
        levels_count = 5;
        switch (level)
        {
            case 0:
                shapes_count = 0; level++; SetNewShape(); return;
                level_00(); break;
            case 1: // Бетон
                shapes_count = 0; level++; SetNewShape(); return;
                level_01();  break;
            case 2: // Бомба
                shapes_count = 0; level++; SetNewShape(); return;
                level_02();  break;
            case 3: // Алмазик
                //shapes_count = 0; level++; SetNewShape(); return;
                level_03();  break;
            case 4:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_04(); break;
            case 5:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_05(); break;
            case 6:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_06(); break;
            case 7:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_07(); break;
            case 8:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_08(); break;
            case 9:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_09(); break;
            case 10:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_10(); break;
            case 11:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_11(); break;
            case 12:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_12(); break;
            case 13:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_13(); break;
            case 14: // Гуливер - 1
                //shapes_count = 0; level++; SetNewShape(); return;
                level_14(); break;
            case 15: // Гуливер - 2
                //shapes_count = 0; level++; SetNewShape(); return;
                level_15(); break;
            case 16: // Гуливер - 3
                //shapes_count = 0; level++; SetNewShape(); return;
                level_16(); break;
            case 17: // Гуливер - 4
                //shapes_count = 0; level++; SetNewShape(); return;
                level_17(); break;
            case 18: // Гуливер - 5
                //shapes_count = 0; level++; SetNewShape(); return;
                level_18(); break;
            case 19: // Гуливер - 6
                //shapes_count = 0; level++; SetNewShape(); return;
                level_19();  break;
            case 20: // Победный алмаз
                level_20();  break;
        }
        if (!Main.stop_game && main.N_shape != -1) { main.RenderNewShape(); }
    }

    private void level_00()
    {
        if (shapes_count > 5) { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        else { main.MakeNewClassicShape(); }
        level_progress = shapes_count / 5f;
    }
    private void level_01() // Бетон
    {
        if (shapes_count == 0) { Instantiate(level01_anime); } // Запуск 01-й анимации 
        if (shapes_count < 20) { main.MakeBetonBlock(1); }
        else if (shapes_count < 40) { main.MakeBetonBlock(2); }
        else if (shapes_count < 60) { main.MakeBetonBlock(3); }
        else if (shapes_count < 80) { main.MakeBetonBlock(4); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 80f;
    }
    private void level_02() // Бомбы
    {
        if (shapes_count == 0) { Instantiate(level02_anime); } // Запуск 02-й анимации 
        if (shapes_count == 0 || shapes_count == 1) { main.MakeBombWhite(); }
        else if (shapes_count == 2 || shapes_count == 3 || shapes_count == 10 || shapes_count == 20 ||
            shapes_count == 30 || shapes_count == 40 || shapes_count == 50) { main.MakeBombBlack(); }
        else if (shapes_count < 60) { main.MakeBetonBlock(4); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 60f;
    }
    private void level_03() // Алмазик
    {
        if (shapes_count == 0)
        {
            vip_shape = main.N_shape;
            main.MakeDiamond();
        }
        else
        {
            if (vip_shape != -1 || FindObjectInSpace("diamond") > 0)
            {
                ChooseRandomShape();
            }
            else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        }
        level_progress = (20f - FindObjectInSpace("diamond")) / 20f;
    }
    private void level_04() //1
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_05() //2
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_06() //3
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_07() //4
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_08() //5
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_09() //6
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_10() //7
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_11() //8
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_12() //9
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_13() //10
    {
        level++; shapes_count = 0; level_progress = 0; SetNewShape(); return;
    }
    private void level_14() // Гулливер - 1
    {
        if (shapes_count == 0) { Instantiate(levelGandalf_anime); main.MakeClassicShape2x2(3); }
        else if (shapes_count < 50) { main.MakeNewClassicShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }

    private void level_15() // Гулливер - 2
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(2); }
        else if (shapes_count < 50) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }
    private void level_16() // Гулливер - 3
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(4); }
        else if (shapes_count < 50) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }
    private void level_17() // Гулливер - 4
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(5); }
        else if (shapes_count < 50) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }
    private void level_18() // Гулливер - 5
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(1); }
        else if (shapes_count < 50) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }
    private void level_19() // Гулливер - 6
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(6); }
        else if (shapes_count < 50) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 50f;
    }
    private void level_20() // Победный алмаз
    {
        if (shapes_count == 0)
        {
            
            vip_shape = main.N_shape;
            main.MakeDiamond2x2();
        }
        else
        {
            if (vip_shape != -1 || FindObjectInSpace("2x2 -r-d diamond") > 0)
            {
                ChooseRandomShape();
            }
            else { Win(); return; }
        }
        level_progress = (20f - FindObjectInSpace("2x2 -r-d diamond")) / 20f * 100f;
    }

    private void ChooseRandomShape()
    {
        Dictionary<string, int> possible_shapes = new Dictionary<string, int>() { { "Empty", 0 } };
        switch (level)
        {
            case 3:
                possible_shapes = new Dictionary<string, int>() { { "Classic", 100 }, { "Beton", 50 } };
                break;
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
                possible_shapes = new Dictionary<string, int>() { { "Classic", 100 }, { "Beton", 20 }, { "BoxBombBlack", 5 } };
                break;
            case 20:
                possible_shapes = new Dictionary<string, int>() { { "Classic", 100 }, { "Beton", 20 }, { "BoxBombBlack", 3 } };
                break;
        }
        int totalWeight = possible_shapes.Values.Sum();
        // Выбираем случайную фигуру с учетом вероятностей.
        int randomNumber = Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (KeyValuePair<string, int> shape in possible_shapes)
        {
            currentWeight += shape.Value;
            if (randomNumber < currentWeight)
            {
                switch (shape.Key)
                {
                    case "Classic":
                        main.MakeNewClassicShape();
                        break;
                    case "Beton":
                        main.MakeBetonBlock(Random.Range(1, 5));
                        break;
                    case "BoxBombBlack":
                        main.MakeBoxBombBlack();
                        break;
                }
                return; 
            }
        }
    }
    private int FindObjectInSpace(string type_of_cell)
    {
        for (int y = 1; y < 40; y++)
        {
            for (int x = 1; x < 15; x++)
            {
                if (main.space_cells_status[x, y] == type_of_cell) { return y; }
            }
        }
        return 0;
    }




    public void StartGame()
    {
        level_progress = 0;
        // Запуск игры. Подготавливаем поле
        main.PrepareGame();
        startWindow.SetActive(false); // Скрываем заставку
        game_interface.SetActive(true); // Отображаем канву с джойстиками (для мобильной версии)

        main.N_shape = 0; // Запускаем левую фигуру классического тетриса
        main.MakeNewClassicShape();
        main.RenderNewShape();
        main.N_shape = 1; // Запускаем правую фигуру классического тетриса
        main.MakeNewClassicShape();
        main.RenderNewShape();
        Instantiate(level00_anime); // Запуск 00-й анимации с космонавтом
    }

    public void GameOver()
    {
        sound_background.Stop();
        sound_defeat.Play();
        game_interface.SetActive(false); // Скрываем канву с джойстиками
        defeatWindow.SetActive(true); // Отображаем канву поражения
        if (level == 0) { Result.text = ""; }
        else { Result.text = ((level - 1 + level_progress) / levels_count * 100f).ToString("F2") + " %"; } // Вывод достугнутого результата 
        Main.stop_game = true; // Останавливаем движение фигур
    }
    public void Win()
    {
        sound_background.Stop();
        sound_victory.Play();
        game_interface.SetActive(false); // Скрываем канву с джойстиками
        victoryWindow.SetActive(true); // Отображаем канву победы
        Main.stop_game = true; // Останавливаем движение фигур
    }
}
