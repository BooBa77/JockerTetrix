using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour, IPointerClickHandler
{
    //[SerializeField] Main main; // Ссылка на скрипт с переменной
    [SerializeField] Sprite pause_pressed; // Спрайт кнопки нажатой паузы
    [SerializeField] Sprite pause_resumed; // Спрайт кнопки отжатой паузы
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Main.stop_game)
        {
            // Если игра сейчас на паузе, то снимаем
            this.GetComponent<Image>().overrideSprite = pause_resumed;
            Main.stop_game = false;
        }
        else 
        {
            // Если нет, то ставим на паузу
            this.GetComponent<Image>().overrideSprite = pause_pressed;
            Main.stop_game = true;
        }
    }
}
