using UnityEngine;
using UnityEngine.U2D;

public class Projector : MonoBehaviour
{
    public Vector2 targetPosition; // ���������� ���� ����������
    public bool waitTimer; // ��������� �� ����� ������? ������������ ����� BonusManager �����, ��� ��������� ���������
    [SerializeField] SpriteShapeController spriteShapeController;
    [SerializeField] GameObject sign; // ����, ������������� �����������
    private float duration = 10f; // �����������������
    private Vector2 lightSourcePosition; // ���������� ��������� �����
    private Vector2 point1Position; // ���������� 1 ����� ������� ����� ����� ����������
    private Vector2 point2Position; // ���������� 2 ����� ������� ����� ����� ����������
    private float radius = 7.0f / 12.0f; // ������ ����������

    void Start()
    {
        spriteShapeController = GetComponent<SpriteShapeController>(); // ����������� ��������� SpriteShapeController. ����� ������� ��� ��� �����
        lightSourcePosition = new Vector2(spriteShapeController.spline.GetPosition(0).x, spriteShapeController.spline.GetPosition(0).y); // ������� ����� - ��� �������� ����� ����������
        waitTimer = true;
    }

    public void EnableProjector() // �������� ���������
    {
        gameObject.SetActive(true); // ��������� �������
        DrawSign();
        // ��������� ������ ��� ���������� ����������
        Invoke("FlashProjector", duration - 1f);
        Invoke("FlashProjector", duration - .8f);
        Invoke("FlashProjector", duration - .6f);
        Invoke("FlashProjector", duration - .4f);
        Invoke("FlashProjector", duration - .2f);
        Invoke("DisableGameObject", duration);
    }
    public void ReplaceSign(Vector2 newPosition) // ������� ����� ��-�� �������� ������
    {
        targetPosition = newPosition;
        DrawSign();
    }
    private void DrawSign() // ����������� ����� � ���� �� ����
    {
        sign.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0); // ������� �����

        // ������������� ������� ������
        // ������ ����� ������� point1Position � point2Position
        FindTangents();
        // ������������� ������� ������ spriteShapeController
        spriteShapeController.spline.SetPosition(1, point1Position);
        spriteShapeController.spline.SetPosition(2, point2Position);
    }

    private void FindTangents()
    {
        // ������ ������ �� ��������� ����� �� ������ ����������
        Vector2 centerVector = new Vector2(targetPosition.x - lightSourcePosition.x, targetPosition.y - lightSourcePosition.y);
        centerVector.Normalize(); // ����������� ������

        // ������ ���������������� ������
        Vector2 tangentVector = new Vector2(-centerVector.y, centerVector.x); // ������, ���������������� centerVector

        // ������������ ���������������� ������ �� ������� ����������
        tangentVector *= radius;

        // ������ ����� �������
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