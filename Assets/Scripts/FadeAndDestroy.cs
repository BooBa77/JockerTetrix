using System.Collections;
using UnityEngine;


public class FadeAndDestroy : MonoBehaviour
{
    private float fadeDuration = 20f; // Длительность затухания
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Запускаем корутину для затухания и удаления
        StartCoroutine(FadeAndDestroyObject());
    }

    IEnumerator FadeAndDestroyObject()
    {
        // Запоминаем начальную прозрачность (alpha)
        float initialAlpha = spriteRenderer.color.a;

        // Плавно уменьшаем прозрачность
        float t = 0f;
        while (t < 1f)
        {
            if (!Main.stop_game) // Если игра не на паузе, продолжаем затухание
            {
                t += Time.deltaTime / fadeDuration;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(initialAlpha, 0f, t));
            }
            yield return null;
        }

        // Делаем спрайт полностью прозрачным
        spriteRenderer.enabled = false;

        // Удаляем объект
        Destroy(gameObject);
    }
}