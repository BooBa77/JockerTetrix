using UnityEngine;
using UnityEngine.UI;

public class Level_01 : MonoBehaviour
{
    [SerializeField] Image jocker1;
    [SerializeField] Image jocker2;
    [SerializeField] Image jockerText;
    private Vector2 startPosition1; // ��������� ������� ������� ��������
    private Vector2 startPosition2; // ��������� ������� ������� ���������
    public Vector2 targetPosition1 = new Vector2(-240, -207); // ������� ������� ��������
    public Vector2 targetPosition2 = new Vector2(223, -202); // ������� ������� ���������
    private float durationEnter = 0.2f; // ����� �� ��������� ��������
    private float durationText = 2f; // ����� ����������� ������
    private float durationExit = 0.5f; // ����� �� ���� ��������
    private float startTime; // ����� ������ ��������

    void Start()
    {
        // ��������� ��������� Image ��������
        startPosition1 = jocker1.rectTransform.anchoredPosition;
        startPosition2 = jocker2.rectTransform.anchoredPosition;
        startTime = Time.time; // ����� ������ ��������
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationExit) // ���� ����� ����� �����, ���� � �����
        {
            Destroy(gameObject); // ���������� Canvas, �� ������� ���� ������
        }
        else if (Time.time - startTime >= durationEnter + durationText) // ���� ����� ����� ������ � ����
        {
            // ������ ��������� � ������
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                jocker1.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition1, startPosition1, progress);
                jocker2.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition2, startPosition2, progress);
            }
        }
        else if (Time.time - startTime >= durationEnter) // ���� ����� ����� ������
        {
            // ������ ������� 2 �������
            jockerText.enabled = true;
        }
        else
        {
            // ��������� �������� ������ ������� �� �����
            float progress = (Time.time - startTime) / durationEnter;
            if (progress <= 1)
            {
                jocker1.rectTransform.anchoredPosition = Vector2.Lerp(startPosition1, targetPosition1, progress);
                jocker2.rectTransform.anchoredPosition = Vector2.Lerp(startPosition2, targetPosition2, progress);
            }
        }
    }
}
