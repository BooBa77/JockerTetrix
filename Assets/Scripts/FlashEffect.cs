using UnityEngine;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    private float duration = 0.15f; // ������������ ���������
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // ��������� �������� ��� ��������� � ��������
        StartCoroutine(FlashAndDestroyObject());
    }

    IEnumerator FlashAndDestroyObject()
    {
        // ���������� ��������� ������������ (alpha)
        float initialAlpha = spriteRenderer.color.a;

        // ������ ��������� ������������
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale += Vector3.one * t * 3f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(initialAlpha, 0f, t));
            yield return null;
        }
        // ������ ������ ��������� ����������
        spriteRenderer.enabled = false;

        // ������� ������
        Destroy(gameObject);
    }
}