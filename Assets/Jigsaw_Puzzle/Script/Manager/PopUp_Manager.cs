using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class PopUp
{
    public string title = "My Title";
    public string message = "My Message";
    public float fadeInDuration = 1.0f;
    public Color pozitifButtonColor = Color.white;
    public string pozitifButtonTextString = "Yes";
    public UnityAction pozitifUnityAction = null;
    public Color negatifButtonColor = Color.white;
    public string negatifButtonTextString = "No";
    public UnityAction negatifUnityAction = null;
    public UnityAction<int> inputUnityAction = null;
}
public class PopUp_Manager : Singletion<PopUp_Manager>
{
    [Header("Genel Script Atamaları")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject clickerStoper;
    [SerializeField] private RectTransform butonsTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    private bool isActive = false;
    private PopUp myPopUp = new PopUp();
    private PopUp myUsingPopUp;
    private Queue<PopUp> popUps = new Queue<PopUp>();

    [Header("Pozitif Button Atamaları")]
    [SerializeField] private Button pozitifButton;
    [SerializeField] private Image pozitifButtonImage;
    [SerializeField] private TextMeshProUGUI pozitifButtonText;

    [Header("Negatif Button Atamaları")]
    [SerializeField] private Button negatifButton;
    [SerializeField] private Image negatifButtonImage;
    [SerializeField] private TextMeshProUGUI negatifButtonText;

    [Header("Slot Atamaları")]
    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI slotAmountText;

    private IEnumerator FadeTimer;
    private void Start()
    {
        FadeTimer = FadeTime(myPopUp.fadeInDuration);
        pozitifButton.onClick.AddListener(PopUpPozitifAnswer);
        negatifButton.onClick.AddListener(PopUpNegatifAnswer);
    }
    public PopUp_Manager SetDesc(string message)
    {
        messageText.text = message;
        return Instance;
    }
    public PopUp_Manager SetCloseButton(bool isOpen)
    {
        closeButton.SetActive(isOpen);
        return Instance;
    }
    // PopUp Panel Ayarlandıktan sonra paneli göstermek için çağrılır. Bu yüzden en son yazılması gerekir.
    public PopUp_Manager PopUpPanelGoster()
    {
        clickerStoper.SetActive(true);
        popUps.Enqueue(myPopUp);
        // Temizle Herşeyi
        myPopUp = new PopUp();
        if (!isActive)
        {
            SiradakiPopUpGoster();
        }
        //Audio_Manager.Instance.PlayPop_UpSourceMusic();
        //Canvas_Manager.Instance.SetClickHolder(true);
        return Instance;
    }
    public void PopUpPanelKapat()
    {
        PopUpPanelSakla();
    }
    public PopUp_Manager SetTitle(string title)
    {
        myPopUp.title = title;
        return Instance;
    }
    public PopUp_Manager SetMessage(string message)
    {
        myPopUp.message = message;
        return Instance;
    }
    public PopUp_Manager SetFadeInDuration(float duration)
    {
        myPopUp.fadeInDuration = duration;
        return Instance;
    }
    private IEnumerator FadeTime(float duration)
    {
        float startingTime = Time.time;
        float alphaTime = 0.0f;
        while (alphaTime < 1)
        {
            alphaTime = Mathf.Lerp(0.0f, 1.0f, (Time.time - startingTime) / duration);
            canvasGroup.alpha = alphaTime;
            yield return null;
        }
    }
    public PopUp_Manager ItemSlot(Sprite item, string slotAmount)
    {
        slotImage.gameObject.SetActive(true);
        butonsTransform.anchoredPosition = new Vector2(butonsTransform.anchoredPosition.x, -100);
        slotImage.sprite = item;
        slotAmountText.text = slotAmount;
        return Instance;
    }
    public PopUp_Manager SetPozitifButtonText(string pozitifText)
    {
        myPopUp.pozitifButtonTextString = pozitifText;
        return Instance;
    }
    public PopUp_Manager SetNegatifButtonText(string negatifText)
    {
        myPopUp.negatifButtonTextString = negatifText;
        return Instance;
    }
    public PopUp_Manager SetPozitifButtonActiver(bool isActive)
    {
        pozitifButton.gameObject.SetActive(isActive);
        return Instance;
    }
    public PopUp_Manager SetNegatifButtonActiver(bool isActive)
    {
        negatifButton.gameObject.SetActive(isActive);
        return Instance;
    }
    public PopUp_Manager SetPozitifButtonColor(Color pozitifColor)
    {
        myPopUp.pozitifButtonColor = pozitifColor;
        return Instance;
    }
    public PopUp_Manager SetNegatifButtonColor(Color negatifColor)
    {
        myPopUp.negatifButtonColor = negatifColor;
        return Instance;
    }
    public PopUp_Manager SetPozitifAction(UnityAction pozitifAction)
    {
        myPopUp.pozitifUnityAction = pozitifAction;
        return Instance;
    }
    public PopUp_Manager SetNegatifAction(UnityAction negatifAction)
    {
        myPopUp.negatifUnityAction = negatifAction;
        return Instance;
    }
    private void PopUpPozitifAnswer()
    {
        myUsingPopUp.pozitifUnityAction?.Invoke();

        //Audio_Manager.Instance.PlayUISourceMusic();
        PopUpPanelSakla();
    }
    private void PopUpNegatifAnswer()
    {
        myUsingPopUp.negatifUnityAction?.Invoke();

        //Audio_Manager.Instance.PlayUISourceMusic();
        PopUpPanelSakla();
    }
    private void SiradakiPopUpGoster()
    {
        myUsingPopUp = popUps.Dequeue();

        titleText.text = myUsingPopUp.title;
        messageText.text = myUsingPopUp.message;
        pozitifButtonImage.color = myUsingPopUp.pozitifButtonColor;
        negatifButtonImage.color = myUsingPopUp.negatifButtonColor;
        pozitifButtonText.text = myUsingPopUp.pozitifButtonTextString;
        negatifButtonText.text = myUsingPopUp.negatifButtonTextString;

        isActive = true;
        canvasGroup.gameObject.SetActive(true);
        StartCoroutine(FadeTime(myUsingPopUp.fadeInDuration));
    }
    private void PopUpPanelSakla()
    {
        isActive = false;
        SetCloseButton(false);
        clickerStoper.SetActive(false);
        StopCoroutine(FadeTimer);
        slotImage.gameObject.SetActive(false);
        canvasGroup.gameObject.SetActive(false);
        //Canvas_Manager.Instance.SetClickHolder(false);

        if (popUps.Count > 0)
        {
            SiradakiPopUpGoster();
        }
    }
}