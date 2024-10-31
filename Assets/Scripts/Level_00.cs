using UnityEngine;
using UnityEngine.UI;

public class Level_00 : MonoBehaviour
{
    [SerializeField] Image jocker;
    [SerializeField] Image jockerText;
    private Vector2 startPosition; // ��������� ������� �������
    public Vector2 targetPosition = new Vector2(-96, -175); // ������� ���������� �������
    private float durationEnter = 0.2f; // ����� ������ �������
    private float durationText = 1f; // ����� ����������� ������
    private float durationFly = 0.5f; // ����� ������ �����
    private float startTime; // ����� ������ ��������

    void Start()
    {
        // ��������� ��������� Image �������
        startPosition = jocker.rectTransform.anchoredPosition;
        startTime = Time.time; // ����� ������ ��������
    }
    void Update()
    {
        if (Time.time - startTime >= durationEnter + durationText + durationFly) // ���� ����� ����� ������, ���� � ����
        {
            Destroy(gameObject); // ���������� Canvas, �� ������� ���� ������
        }
        else if (Time.time - startTime >= durationEnter + durationText) // ���� ����� ����� ������ � ����
        {
            // ������ ��������� � �������
            jockerText.enabled = false;
            float progress = (Time.time - startTime - durationEnter - durationText) / durationFly;
            if (progress <= 1)
            {
                jocker.rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, Vector2.up * Screen.height, progress); // �������
                jocker.rectTransform.localScale = Vector3.Lerp(jocker.rectTransform.localScale, new Vector3(0.3f, 0.3f, 1f), progress); // �������
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
