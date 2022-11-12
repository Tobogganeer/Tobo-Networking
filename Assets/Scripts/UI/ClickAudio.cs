using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickAudio : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private static float lastHoverAudioTime;
    private static float lastClickAudioTime;
    private const float CLICK_MIN_DELAY = 0.06f;
    private const float HOVER_MIN_DELAY = 0.035f;

    private const float HOVER_VOLUME = 0.2f;
    private const float CLICK_VOLUME = 0.1f;

    public static void Hover()
    {
        if (lastHoverAudioTime - Time.time < -HOVER_MIN_DELAY)
        {
            AudioManager.PlayLocal(new Audio("UIHover").SetVolume(HOVER_VOLUME));
            //AudioManager.Play2DLocal(AudioArray.UIHover, AudioCategory.SFX, HOVER_VOLUME);
            lastHoverAudioTime = Time.time;
        }
    }

    public static void Click()
    {
        if (lastClickAudioTime - Time.time < -CLICK_MIN_DELAY)
        {
            AudioManager.PlayLocal(new Audio("UIClick").SetVolume(CLICK_VOLUME));
            //AudioManager.Play2DLocal(AudioArray.UIClick, AudioCategory.SFX, CLICK_VOLUME);
            lastClickAudioTime = Time.time;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
    }
}
