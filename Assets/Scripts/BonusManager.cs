using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BonusManager : MonoBehaviour
{
    public int defaultTimerBatman; // ������� � ������� ������� ���� �������
    [SerializeField] LevelManager levelManager; // ������ �� ������, ���������� �������� ����
    [SerializeField] Main main; // ������ �� �������� ������ ����
    public GameObject[] bonusLeft; // ������ ������ ������� ������
    public GameObject[] bonusRight; // ������ ������ ������� �������
    public Sprite[] bonusSprite; // ������ ������ �������
    public int bonusCountLeft; // ���������� ������� ��������� ����� �������
    public int bonusCountRight; // ���������� ������� ��������� ������ �������

    [SerializeField] Projector projectorLeftScript; // ������ �� ������ ������ ����������
    private float projectorLeftTimer; // ������ ������ ����������
    public Vector2 projectorLeftPosition; // ���������� ���� ������ ����������, ���� (0,0) ������ ��������
    [SerializeField] Projector projectorRightScript; // ������ �� ������ ������� ����������
    public float projectorRightTimer; // ������ ������� ����������
    public Vector2 projectorRightPosition; // ���������� ���� ������� ����������, ���� (0,0) ������ ��������

    void Start()
    {
        projectorLeftTimer = NewTimer(defaultTimerBatman); projectorLeftScript.waitTimer = false;
        projectorRightTimer = NewTimer(defaultTimerBatman); projectorRightScript.waitTimer = false;

    }

    void Update()
    {
        if (Main.stop_game) { return; } // ���� ���� �����������, ��������

        // ������� ������ 
        if (Input.GetKeyDown(KeyCode.Q) && bonusCountLeft > 0) { UseBonus(0, 0); } // ������������� 1 ������ ������


        if (projectorLeftScript.targetPosition.x == 0f) // ����� ��������� ���� �� �������
        {
            projectorLeftTimer -= Time.deltaTime; // ���������� ����� �������
            if (projectorLeftTimer < 0) // ����� ������ ����� ������� �������
            {
                if (projectorLeftScript.waitTimer == true)
                {
                    projectorLeftPosition = new Vector2(0f, -10f);
                    projectorLeftTimer = NewTimer(defaultTimerBatman); // ��������� ����� ������
                    projectorLeftScript.waitTimer = false;
                }
                else
                {
                    Vector2 placeSign = FindPlaceForSign(); // ���������� ����������� ������
                    if (placeSign.x == 0) { return; } // ��� ���������� �����
                    projectorLeftScript.targetPosition = placeSign; // ��������� ����� ��������� �� ��������� ������
                    projectorLeftScript.EnableProjector(); // ������� ����� ���������
                    projectorLeftPosition = placeSign;
                }
            }
        }

        if (projectorRightScript.targetPosition.x == 0f) // ������ ��������� ���� �� �������
        { 
            projectorRightTimer -= Time.deltaTime; // ���������� ����� �������
            if (projectorRightTimer < 0) // ������ ������ ����� ������� �������
            {
                if (projectorRightScript.waitTimer == true)
                {
                    projectorRightPosition = new Vector2(0f, -10f);
                    projectorRightTimer = NewTimer(defaultTimerBatman); // ��������� ����� ������
                    projectorRightScript.waitTimer = false;
                }
                else
                {
                    Vector2 placeSign = FindPlaceForSign(); // ���������� ����������� ������
                    if (placeSign.x == 0) { return; } // ��� ���������� �����
                    projectorRightScript.targetPosition = placeSign; // ���������� ������ ��������� �� ��������� ������
                    projectorRightScript.EnableProjector(); // ������� ������ ���������
                    projectorRightPosition = placeSign;
                }
            }
        }
    }
    private int NewTimer(int defaultTimer) // ����� ������ ��������� ����������
    {
        return defaultTimer; // ���� ��-���������
    }

    private Vector2 FindPlaceForSign() // ����� ���������� ����������� ������
    {
        Dictionary<int, Vector2> signPositions = new Dictionary<int, Vector2>(); // ������� ���������� �����
        for (int y = 1; y < 40; y++)
        {
            for (int x = 1; x < 15; x++)
            {
                if (main.space_cells_status[x, y] == "classic" // ������ ��� ����������� ����� ���� ������ ������ ���� "classic"
                    && (x != projectorLeftPosition.x && y != projectorLeftPosition.y) // � �� ������� �����
                    && (x != projectorRightPosition.x && y != projectorRightPosition.y)) // ��� ������ �����������
                {
                    signPositions.Add(signPositions.Count, new Vector2(x, y)); // ��������� � ������� ���������� ��������� ������� ��� �����
                }
            }
        }
        if (signPositions.Count > 0) // ���� ���������� ������ �������
        {
            int randomIndex = Random.Range(0, signPositions.Count); // �������� ��������� ������ �� ������
            return signPositions[randomIndex]; // ���������� ���������� ���������� ������
        }
        return (new Vector2(0, 0)); // ������ �� �������
    }

    public void TakeBonus(Vector2 bonusPosition, int N_shape)
    {
        if (bonusPosition == projectorLeftPosition)
        {
            projectorLeftScript.DisableGameObject();
            projectorLeftScript.ReplaceSign(new Vector2(0f, -10f)); // ����� �� ������ ���������� �������� ����������
            projectorLeftPosition = new Vector2(0f, -10f); // ��������� ���������
            projectorLeftTimer = NewTimer(defaultTimerBatman); // ��������� ����� ������
        }
        if (bonusPosition == projectorRightPosition)
        {
            projectorRightScript.DisableGameObject();
            projectorRightScript.ReplaceSign(new Vector2(0f, -10f)); // ����� �� ������ ���������� �������� ����������
            projectorRightPosition = new Vector2(0f, -10f); // ��������� ���������
            projectorRightTimer = NewTimer(defaultTimerBatman); // ��������� ����� ������
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

    public void ReplaceSign(Vector2 oldPosition, Vector2 newPosition) // ������� ����� ��-�� �������� ������
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
