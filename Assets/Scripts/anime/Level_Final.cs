using UnityEngine;
using UnityEngine.UI;

public class Level_Final : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    public Vector2 startPosition; // ��������� ������� ������� �������
    public Vector2 targetPosition; // �������� ������� ������� �������
    public Vector2 endPosition; // ������� ������� ������� � ����� ��������
    private float durationEnter = 0.2f; // ����� �� ��������� �������
    private float durationText = 2f; // ����� ����������� ������
    private float durationExit = 0.5f; // ����� �� ���� �������
    private float startTime; // ����� ������ ��������

    void Start()
    {
        // ��������� ��������� Image �������
        startPosition = jocker.rectTransform.anchoredPosition;
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
            // ������ ��������� � ������ � �������
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationExit;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, endPosition, progress);
                jocker.rectTransform.localScale = Vector3.Lerp(jocker.rectTransform.localScale, new Vector3(1f, 1f, 1f), progress); // �������
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
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            }
        }
    }
}
