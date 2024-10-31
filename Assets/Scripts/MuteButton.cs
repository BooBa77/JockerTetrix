using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] LevelManager lm; // Ссылка на скрипт с фоновой музыкой
    [SerializeField] Sprite mute_pressed; // Спрайт кнопки с выключенной музыкой
    [SerializeField] Sprite mute_resumed; // Спрайт кнопки с включенной музыкой
    public void OnPointerClick(PointerEventData eventData)
    {
        if (lm.sound_background.isPlaying)
        {
            // Если музыка играет, ставим на паузу
            this.GetComponent<Image>().overrideSprite = mute_pressed;
            lm.sound_background.Pause();
        }
        else
        {
            // Если нет, то включаем
            this.GetComponent<Image>().overrideSprite = mute_resumed;
            lm.sound_background.Play();
        }
    }
}

