using System.Collections;
using UnityEngine;


public class FadeAndDestroy : MonoBehaviour
{
    private float fadeDuration = 20f; // ������������ ���������
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // ��������� �������� ��� ��������� � ��������
        StartCoroutine(FadeAndDestroyObject());
    }

    IEnumerator FadeAndDestroyObject()
    {
        // ���������� ��������� ������������ (alpha)
        float initialAlpha = spriteRenderer.color.a;

        // ������ ��������� ������������
        float t = 0f;
        while (t < 1f)
        {
            if (!Main.stop_game) // ���� ���� �� �� �����, ���������� ���������
            {
                t += Time.deltaTime / fadeDuration;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(initialAlpha, 0f, t));
            }
            yield return null;
        }

        // ������ ������ ��������� ����������
        spriteRenderer.enabled = false;

        // ������� ������
        Destroy(gameObject);
    }
}