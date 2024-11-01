using UnityEngine;
using UnityEngine.UI;

public class Level_Gandalf : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    public Vector2 startPosition; // ��������� ������� �������
    public Vector2 targetPosition; // ������� �������
    private float durationEnter = 0.2f; // ����� �� ��������� �������
    private float durationText = 2f; // ����� ����������� ������
    private float durationExit = 0.5f; // ����� �� ���� �������
    private float startTime; // ����� ������ ��������

    void Start()
    {
        // ��������� ��������� Image ��������
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
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, startPosition, progress);
            }
        }
        else if (Time.time - startTime >= durationEnter) // ���� ����� ����� ������
        {
            // ������ �������
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
