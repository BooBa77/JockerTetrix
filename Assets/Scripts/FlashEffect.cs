using UnityEngine;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    private float duration = 0.15f; // Длительность затухания
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Запускаем корутину для затухания и удаления
        StartCoroutine(FlashAndDestroyObject());
    }

    IEnumerator FlashAndDestroyObject()
    {
        // Запоминаем начальную прозрачность (alpha)
        float initialAlpha = spriteRenderer.color.a;

        // Плавно уменьшаем прозрачность
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale += Vector3.one * t * 3f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(initialAlpha, 0f, t));
            yield return null;
        }
        // Делаем спрайт полностью прозрачным
        spriteRenderer.enabled = false;

        // Удаляем объект
        Destroy(gameObject);
    }
}