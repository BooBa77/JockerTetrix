using UnityEngine;
using UnityEngine.UI;

public class Level_02 : MonoBehaviour
{
    [SerializeField] Image Batman; // �������� ������� � ����
    [SerializeField] Sprite batmanBack; // ������ ������� �� �����
    [SerializeField] Image Bomb1; // �������� ����
    [SerializeField] Image Bomb2;
    private Vector2 startPosition; // ��������� ������� �������
    public Vector2 targetPosition = new Vector2(66, -148); // �������� ������� �������
    public Vector2 finishPosition = new Vector2(56, 508); // ������� ������� � ����� �����
    private float durationEnter = 0.2f; // ����� �� ��������� �������
    private float durationText = 2f; // ����� �����������
    private float durationExit = 0.5f; // ����� �� ���� �������
    private float startTime; // ����� ������ ��������

    void Start()
    {
        // ��������� ��������� Image �������
        startPosition = Batman.rectTransform.anchoredPosition;
        startTime = Time.time; // ����� ������ ��������
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationExit) // ���� ����� ����� �����, ����������� � �����
        {
            Destroy(gameObject); // ���������� Canvas, �� ������� ���� ������
        }
        else if (Time.time - startTime >= durationEnter + durationText) // ���� ����� ����� ������ � �����������
        {
            // ������ ������� � ������
            Bomb1.enabled = false; Bomb2.enabled = false;
            Batman.sprite = batmanBack;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                Batman.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, finishPosition, progress);
            }
        }
        else if (Time.time - startTime >= durationEnter) // ���� ����� ����� ������
        {
            // Batman ������ ����� � �����
            Bomb1.enabled = true; Bomb2.enabled = true;
        }
        else
        {
            // ��������� �������� ������ ������� �� �����
            float progress = (Time.time - startTime) / durationEnter;
            if (progress <= 1)
            {
                Batman.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            }
        }
    }
}
