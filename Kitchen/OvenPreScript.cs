using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OvenPreScript : MonoBehaviour
{
    public OvenState ovenState;

    string objectName;
    public int ovenID;

    public GameObject bakeryItem;

    public DateTime startTime;
    public DateTime finishTime; // 완료 예정 시간
    public float saveTime; // 아낀시간

    public Image slider;
    public Button finishButton;

    private Image sliderBar;
    private TextMeshProUGUI sliderTime;

    void Awake()
    {
        ovenState = OvenState.Idle;
        objectName = gameObject.name;

        sliderBar = slider.transform.Find("sliderBar")?.GetComponent<Image>();
        sliderTime = slider.transform.Find("sliderTime")?.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        switch (ovenState)
        {
            case OvenState.Idle:
                Idle();
                break;
            case OvenState.bake:
                Bake();
                break;
            case OvenState.finish:
                Finish();
                break;
        }
    }

    void Idle()
    {
        slider.gameObject.SetActive(false);
        finishButton.gameObject.SetActive(false);
    }

    void Bake()
    {
        slider.gameObject.SetActive(true);
        finishButton.gameObject.SetActive(true);

        if (bakeryItem != null)
            finishButton.image.sprite = bakeryItem.GetComponent<SpriteRenderer>().sprite;
    }

    void Finish()
    {
        slider.gameObject.SetActive(true);
        finishButton.gameObject.SetActive(true);

        if (bakeryItem != null)
            finishButton.image.sprite = bakeryItem.GetComponent<SpriteRenderer>().sprite;

        if (sliderBar != null)
            sliderBar.fillAmount = 1;

        if (sliderTime != null)
            sliderTime.text = "Done";
    }

    public void StartBaking(GameObject bakery, float time)
    {
        bakeryItem = bakery;
        startTime = DateTime.Now;

        float ovenSpeedBonus = KitchenManager.Instance.ovenSpeedLevel;
        float adjustedTime = time * (1f - saveTime / 100f - ovenSpeedBonus / 100f);
        finishTime = startTime.AddSeconds(adjustedTime);

        ovenState = OvenState.bake;
        StartCoroutine(ActivateObject());
    }

    public IEnumerator ActivateObject()
    {
        while (DateTime.Now < finishTime)
        {
            TimeSpan remainingTime = finishTime - DateTime.Now;
            float totalBakeTime = (float)(finishTime - startTime).TotalSeconds;
            float elapsedBakeTime = (float)(DateTime.Now - startTime).TotalSeconds;
            float progress = Mathf.Clamp01(elapsedBakeTime / totalBakeTime);

            if (sliderBar != null)
                sliderBar.fillAmount = progress;

            if (sliderTime != null)
                sliderTime.text = FormatTime(remainingTime);

            yield return null;
        }

        ovenState = OvenState.finish;
        SoundManager.Instance.PlaySFX("OvenFinish");
    }

    string FormatTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{time.Hours}H {time.Minutes:D2}M";
        else if (time.TotalMinutes >= 1)
            return $"{time.Minutes}M {time.Seconds:D2}S";
        else
            return $"{Mathf.Max(0, time.Seconds)}S";
    }

    public void GetItemButton()
    {
        if (ovenState == OvenState.finish)
        {
            GetItem();
        }
        else if (ovenState == OvenState.bake)
        {
            PopupManager.Instance.OpenOvenAdPopup(this);
        }
    }

    void GetItem()
    {
        if (ShowcaseManager.Instance.showcaseStorageCount > ShowcaseManager.Instance.showcaseStorageLimit)
        {
            StartCoroutine(PopupManager.Instance.AlertPopup("alert_showcase"));
            Debug.Log("저장고가 가득찼습니다");
            return;
        }

        ShowcaseManager.Instance.AddItem(bakeryItem);
        SoundManager.Instance.PlaySFX("GetItem");

        bakeryItem = null;
        ovenState = OvenState.Idle;
    }
}
