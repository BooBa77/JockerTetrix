using UnityEngine;
using UnityEngine.U2D;

public class Projector : MonoBehaviour
{
    public Vector2 targetPosition; // Координаты цели прожектора
    public bool waitTimer; // Требуется ли новый таймер? Используется чтобы BonusManager понял, что прожектор отработал
    [SerializeField] SpriteShapeController spriteShapeController;
    [SerializeField] GameObject sign; // Знак, высвечиваемый прожектором
    private float duration = 10f; // Продолжительность
    private Vector2 lightSourcePosition; // Координаты источника света
    private Vector2 point1Position; // Координаты 1 точки касания знака лучом прожектора
    private Vector2 point2Position; // Координаты 2 точки касания знака лучом прожектора
    private float radius = 7.0f / 12.0f; // Радиус окружности

    void Start()
    {
        spriteShapeController = GetComponent<SpriteShapeController>(); // Собственный компонент SpriteShapeController. Будем двигать две его точки
        lightSourcePosition = new Vector2(spriteShapeController.spline.GetPosition(0).x, spriteShapeController.spline.GetPosition(0).y); // Нулевая точка - это источник света прожектора
        waitTimer = true;
    }

    public void EnableProjector() // Включаем прожектор
    {
        gameObject.SetActive(true); // Активация объекта
        DrawSign();
        // Запускаем таймер для отключения прожектора
        Invoke("FlashProjector", duration - 1f);
        Invoke("FlashProjector", duration - .8f);
        Invoke("FlashProjector", duration - .6f);
        Invoke("FlashProjector", duration - .4f);
        Invoke("FlashProjector", duration - .2f);
        Invoke("DisableGameObject", duration);
    }
    public void ReplaceSign(Vector2 newPosition) // Перенос знака из-за смещения ячейки
    {
        targetPosition = newPosition;
        DrawSign();
    }
    private void DrawSign() // Отображение знака и луча до него
    {
        sign.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0); // перенос знака

        // Устанавливаем позиции вершин
        // Найдем точки касания point1Position и point2Position
        FindTangents();
        // Устанавливаем позиции вершин spriteShapeController
        spriteShapeController.spline.SetPosition(1, point1Position);
        spriteShapeController.spline.SetPosition(2, point2Position);
    }

    private void FindTangents()
    {
        // Найдем вектор от источника света до центра окружности
        Vector2 centerVector = new Vector2(targetPosition.x - lightSourcePosition.x, targetPosition.y - lightSourcePosition.y);
        centerVector.Normalize(); // Нормализуем вектор

        // Найдем перпендикулярный вектор
        Vector2 tangentVector = new Vector2(-centerVector.y, centerVector.x); // Вектор, перпендикулярный centerVector

        // Масштабируем перпендикулярный вектор до радиуса окружности
        tangentVector *= radius;

        // Найдем точки касания
        point1Position = new Vector2(targetPosition.x + tangentVector.x, targetPosition.y + tangentVector.y - 3f);
        point2Position = new Vector2(targetPosition.x - tangentVector.x, targetPosition.y - tangentVector.y - 3f);
    }
    private void FlashProjector()
    {
        gameObject.SetActive(false); 
        Invoke("PowerOn", 0.1f);
    }
    private void PowerOn()
    {
        gameObject.SetActive(true);
    }

    public void DisableGameObject()
    {
        targetPosition = new Vector2(0f, -10f);
        waitTimer = true;
        gameObject.SetActive(false);
    }
}