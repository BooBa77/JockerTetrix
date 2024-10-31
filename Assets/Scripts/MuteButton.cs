using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] LevelManager lm; // ������ �� ������ � ������� �������
    [SerializeField] Sprite mute_pressed; // ������ ������ � ����������� �������
    [SerializeField] Sprite mute_resumed; // ������ ������ � ���������� �������
    public void OnPointerClick(PointerEventData eventData)
    {
        if (lm.sound_background.isPlaying)
        {
            // ���� ������ ������, ������ �� �����
            this.GetComponent<Image>().overrideSprite = mute_pressed;
            lm.sound_background.Pause();
        }
        else
        {
            // ���� ���, �� ��������
            this.GetComponent<Image>().overrideSprite = mute_resumed;
            lm.sound_background.Play();
        }
    }
}

