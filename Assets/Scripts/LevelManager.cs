using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Main main; // ������ �� �������� ������ ����
    public int level = 0; // ������� ������� ����
    private int levels_count; // ���������� ������� � ����
    public float level_progress; // �������� ����������� �������� ������
    public Text Result; // ��������� ����, ��������� � ���� ���������
    public int shapes_count = 0; // ������� ������� �� ������� �����
    public int vip_shape = -1; // ����� �������� ������ �����. ������������, ����� ���������� ����� �� ���
    [SerializeField] GameObject startWindow; // ������ ���������
    [SerializeField] GameObject victoryWindow; // ������ ��������
    [SerializeField] GameObject defeatWindow; // ������ ���������
    [SerializeField] GameObject game_interface; // ������ � �����������
    // �����
    public AudioSource sound_background; // ������� ������
    [SerializeField] AudioSource sound_victory; // ������
    [SerializeField] AudioSource sound_defeat; // ���������

    [SerializeField] Text textLabel; // ��������� ���� ��� ������ ���������� �� ������
    private float fadeDuration = 3f; // ������������ ������� ������������ ������
    // �������
    [SerializeField] GameObject level00_anime; // ������ � ��������� 0-�� ������
    [SerializeField] GameObject level01_anime; // ������ � ��������� 1-�� ������
    [SerializeField] GameObject level02_anime; // ������ � ��������� 2-�� ������
    [SerializeField] GameObject levelFinal_anime; // ������ � ��������� ���������� ������


    public void SetNewShape()
    {
        levels_count = 5;
        switch (level)
        {
            case 0:
                //shapes_count = 0; level++; SetNewShape(); return;
                level_00(); break;
            case 1: // �����
                //shapes_count = 0; level++; SetNewShape(); return;
                level_01();  break;
            case 2: // �����
                //shapes_count = 0; level++; SetNewShape(); return;
                level_02();  break;
            case 3: // �������
                //shapes_count = 0; level++; SetNewShape(); return;
                level_03();  break;
            case 4: // �������
                //shapes_count = 0; level++; SetNewShape(); return;
                level_04();  break;
            case 5: // �������� �����
                level_05();  break;
        }
        if (!Main.stop_game && main.N_shape != -1) { main.RenderNewShape(); }
    }

    private void level_00()
    {
        if (shapes_count > 5) { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        else { main.MakeNewClassicShape(); }
        level_progress = shapes_count / 5f;
    }
    private void level_01() // �����
    {
        if (shapes_count == 0) { Instantiate(level01_anime); } // ������ 01-� �������� 
        if (shapes_count < 20) { main.MakeBetonBlock(1); }
        else if (shapes_count < 40) { main.MakeBetonBlock(2); }
        else if (shapes_count < 60) { main.MakeBetonBlock(3); }
        else if (shapes_count < 80) { main.MakeBetonBlock(4); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 80f;
    }
    private void level_02() // �����
    {
        if (shapes_count == 0) { Instantiate(level02_anime); } // ������ 02-� �������� 
        if (shapes_count == 0 || shapes_count == 1) { main.MakeBombWhite(); }
        else if (shapes_count == 2 || shapes_count == 3 || shapes_count == 10 || shapes_count == 20 ||
            shapes_count == 30 || shapes_count == 40 || shapes_count == 50) { main.MakeBombBlack(); }
        else if (shapes_count < 60) { main.MakeBetonBlock(4); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 60f;
    }
    private void level_03() // �������
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
    private void level_04() // ��������
    {
        if (shapes_count == 0) { main.MakeClassicShape2x2(3); }
        else if (shapes_count < 50) { main.MakeNewClassicShape(); }
        else if (shapes_count == 50) { main.MakeClassicShape2x2(2); }
        else if (shapes_count < 100) { ChooseRandomShape(); }
        else if (shapes_count == 100) { main.MakeClassicShape2x2(4); }
        else if (shapes_count < 150) { ChooseRandomShape(); }
        else if (shapes_count == 150) { main.MakeClassicShape2x2(5); }
        else if (shapes_count < 200) { ChooseRandomShape(); }
        else if (shapes_count == 200) { main.MakeClassicShape2x2(1); }
        else if (shapes_count < 250) { ChooseRandomShape(); }
        else if (shapes_count == 250) { main.MakeClassicShape2x2(6); }
        else if (shapes_count == 298) {Instantiate(levelFinal_anime); }  // ������ �������� �������
        else if (shapes_count < 300) { ChooseRandomShape(); }
        //else if (shapes_count == 300) { main.MakeClassicShape2x2(0); }
        //else if (shapes_count < 350) { ChooseRandomShape(); }
        else { level++; shapes_count = 0; level_progress = 0; SetNewShape(); return; }
        level_progress = shapes_count / 350f;
    }
    private void level_05() // �������� �����
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
            case 4:
            case 5:
                possible_shapes = new Dictionary<string, int>() { { "Classic", 100 }, { "Beton", 20 }, { "BoxBombBlack", 5 } };
                break;
        }
        int totalWeight = possible_shapes.Values.Sum();
        // �������� ��������� ������ � ������ ������������.
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
        // ������ ����. �������������� ����
        main.PrepareGame();
        startWindow.SetActive(false); // �������� ��������
        game_interface.SetActive(true); // ���������� ����� � ����������� (��� ��������� ������)

        main.N_shape = 0; // ��������� ����� ������ ������������� �������
        main.MakeNewClassicShape();
        main.RenderNewShape();
        main.N_shape = 1; // ��������� ������ ������ ������������� �������
        main.MakeNewClassicShape();
        main.RenderNewShape();
        Instantiate(level00_anime); // ������ 00-� �������� � �����������
    }

    public void GameOver()
    {
        sound_background.Stop();
        sound_defeat.Play();
        game_interface.SetActive(false); // �������� ����� � �����������
        defeatWindow.SetActive(true); // ���������� ����� ���������
        if (level == 0) { Result.text = ""; }
        else { Result.text = ((level - 1 + level_progress) / levels_count * 100f).ToString("F2") + " %"; } // ����� ������������ ���������� 
        Main.stop_game = true; // ������������� �������� �����
    }
    public void Win()
    {
        sound_background.Stop();
        sound_victory.Play();
        game_interface.SetActive(false); // �������� ����� � �����������
        victoryWindow.SetActive(true); // ���������� ����� ������
        Main.stop_game = true; // ������������� �������� �����
    }
}
