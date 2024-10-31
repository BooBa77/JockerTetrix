using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour, IPointerClickHandler
{
    //[SerializeField] Main main; // ������ �� ������ � ����������
    [SerializeField] Sprite pause_pressed; // ������ ������ ������� �����
    [SerializeField] Sprite pause_resumed; // ������ ������ ������� �����
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Main.stop_game)
        {
            // ���� ���� ������ �� �����, �� �������
            this.GetComponent<Image>().overrideSprite = pause_resumed;
            Main.stop_game = false;
        }
        else 
        {
            // ���� ���, �� ������ �� �����
            this.GetComponent<Image>().overrideSprite = pause_pressed;
            Main.stop_game = true;
        }
    }
}
