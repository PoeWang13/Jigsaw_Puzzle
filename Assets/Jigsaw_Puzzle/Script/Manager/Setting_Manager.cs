using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Setting_Manager : Singletion<Setting_Manager>
{
    private void Start()
    {
        //// Learning Language
        //LearnLanguage();

        //// Learning Move UI and Vibration
        //LearnMoveUI();
        //LearnVibration();

        // All Music Volume
        LearnAllMusic();

        // BackGround Music Volume
        LearnBackGroundMusic();

        // UI Music Volume
        LearnUIMusic();

        // Effect Music Volume
        LearnEffectMusic();

        //// Gun Music Volume
        //LearnGunMusic();

        //// Explosion Music Volume
        //LearnExplosionMusic();

        //// Pop_Up Music Volume
        //LearnPop_UpMusic();

        //// Drop Music Volume
        //LearnDropMusic();
    }
    #region Audio
    [SerializeField] private AudioMixer allMusicMixer;

    #region AllMusic
    [SerializeField] private Slider allMusicSlider;
    private void LearnAllMusic()
    {
        allMusicSlider.value = PlayerPrefs.GetInt("AllMusic", 0);
        allMusicSlider.onValueChanged.AddListener((x) => AllMusicMusic(x));
        SetAllSourceMusic(PlayerPrefs.GetInt("AllMusicSource", 1) == 0 ? true : false);
    }
    private void AllMusicMusic(float volume)
    {
        allMusicMixer.SetFloat("AllMusic", volume);
        PlayerPrefs.SetInt("AllMusic", (int)volume);
    }
    // Setting panelinde all music music açma kapama butonlarýnda atandý.
    public void SetAllSourceMusic(bool isAllMusicSourceMute)
    {
        PlayerPrefs.SetInt("AllMusicSource", isAllMusicSourceMute ? 0 : 1);
        Audio_Manager.Instance.SetAllSourceMusic(isAllMusicSourceMute);
    }
    #endregion

    #region Background
    [SerializeField] private Slider backGroundSlider;
    private void LearnBackGroundMusic()
    {
        if (backGroundSlider != null)
        {
            backGroundSlider.value = PlayerPrefs.GetInt("BackGroundMusic", 0);
            backGroundSlider.onValueChanged.AddListener((x) => BackGroundMusic(x));
            SetAllSourceMusic(PlayerPrefs.GetInt("BackgroundSource", 1) == 0 ? true : false);
        }
    }
    private void BackGroundMusic(float volume)
    {
        allMusicMixer.SetFloat("BackGround", volume);
        PlayerPrefs.SetInt("BackGroundMusic", (int)volume);
    }
    // Setting panelinde background music açma kapama butonlarýnda atandý.
    public void SetBackgroundSourcMusic(bool isBackgroundSourceMute)
    {
        PlayerPrefs.SetInt("BackgroundSource", isBackgroundSourceMute ? 0 : 1);
        Audio_Manager.Instance.SetBackgroundSourcMusic(isBackgroundSourceMute);
    }
    #endregion

    #region UI
    [SerializeField] private Slider uISlider;
    private void LearnUIMusic()
    {
        if (uISlider != null)
        {
            uISlider.value = PlayerPrefs.GetInt("UIMusic", 0);
            uISlider.onValueChanged.AddListener((x) => UIMusic(x));
            SetAllSourceMusic(PlayerPrefs.GetInt("UISource", 1) == 0 ? true : false);
        }
    }
    private void UIMusic(float volume)
    {
        allMusicMixer.SetFloat("UI", volume);
        PlayerPrefs.SetInt("UIMusic", (int)volume);
    }
    // Setting panelinde uI music açma kapama butonlarýnda atandý.
    public void SetUISourceMusic(bool isUISourceMute)
    {
        PlayerPrefs.SetInt("UISource", isUISourceMute ? 0 : 1);
        Audio_Manager.Instance.SetUISourceMusic(isUISourceMute);
    }
    #endregion

    #region Effect
    [SerializeField] private Slider effectSlider;
    private void LearnEffectMusic()
    {
        if (effectSlider != null)
        {
            effectSlider.value = PlayerPrefs.GetInt("EffectMusic", 0);
            effectSlider.onValueChanged.AddListener((x) => EffectMusic(x));
            SetAllSourceMusic(PlayerPrefs.GetInt("EffectSource", 1) == 0 ? true : false);
        }
    }
    private void EffectMusic(float volume)
    {
        allMusicMixer.SetFloat("Effect", volume);
        PlayerPrefs.SetInt("EffectMusic", (int)volume);
    }
    // Setting panelinde effect music açma kapama butonlarýnda atandý.
    public void SetEffectSourceMusic(bool isEffectSourceMute)
    {
        PlayerPrefs.SetInt("EffectSource", isEffectSourceMute ? 0 : 1);
        Audio_Manager.Instance.SetEffectSourceMusic(isEffectSourceMute);
    }
    #endregion

    //#region Gun
    //[SerializeField] private Slider gunSlider;
    //private void LearnGunMusic()
    //{
    //    if (gunSlider != null)
    //    {
    //        gunSlider.value = PlayerPrefs.GetInt("GunMusic", 0);
    //        gunSlider.onValueChanged.AddListener((x) => GunMusic(x));
    //        SetAllSourceMusic(PlayerPrefs.GetInt("GunSource", 0) == 0 ? true : false);
    //    }
    //}
    //private void GunMusic(float volume)
    //{
    //    allMusicMixer.SetFloat("Gun", volume);
    //    PlayerPrefs.SetInt("GunMusic", (int)volume);
    //}
    //// Setting panelinde gun music açma kapama butonlarýnda atandý.
    //public void SetGunSourcelMusic(bool isGunSourceMute)
    //{
    //    PlayerPrefs.SetInt("GunSource", isGunSourceMute ? 0 : 1);
    //    Audio_Manager.Instance.SetGunSourcelMusic(isGunSourceMute);
    //}
    //#endregion

    //#region Explosion 
    //[SerializeField] private Slider explosionSlider;
    //private void LearnExplosionMusic()
    //{
    //    if (explosionSlider != null)
    //    {
    //        explosionSlider.value = PlayerPrefs.GetInt("ExplosionMusic", 0);
    //        explosionSlider.onValueChanged.AddListener((x) => ExplosionMusic(x));
    //        SetAllSourceMusic(PlayerPrefs.GetInt("ExplosionSource", 0) == 0 ? true : false);
    //    }
    //}
    //private void ExplosionMusic(float volume)
    //{
    //    allMusicMixer.SetFloat("Explosion", volume);
    //    PlayerPrefs.SetInt("ExplosionMusic", (int)volume);
    //}
    //// Setting panelinde explosion music açma kapama butonlarýnda atandý.
    //public void SetExplosionSourceMusic(bool isExplosionSourceMute)
    //{
    //    PlayerPrefs.SetInt("ExplosionSource", isExplosionSourceMute ? 0 : 1);
    //    Audio_Manager.Instance.SetExplosionSourceMusic(isExplosionSourceMute);
    //}
    //#endregion

    //#region Pop_Up
    //[SerializeField] private Slider pop_UpSlider;
    //private void LearnPop_UpMusic()
    //{
    //    if (pop_UpSlider != null)
    //    {
    //        pop_UpSlider.value = PlayerPrefs.GetInt("Pop_UpMusic", 0);
    //        pop_UpSlider.onValueChanged.AddListener((x) => Pop_UpMusic(x));
    //        SetAllSourceMusic(PlayerPrefs.GetInt("Pop_UpSource", 0) == 0 ? true : false);
    //    }
    //}
    //private void Pop_UpMusic(float volume)
    //{
    //    allMusicMixer.SetFloat("Pop_Up", volume);
    //    PlayerPrefs.SetInt("Pop_UpMusic", (int)volume);
    //}
    //// Setting panelinde pop_up music açma kapama butonlarýnda atandý.
    //public void SetPop_UpSourceMusic(bool isPop_UpSourceMute)
    //{
    //    PlayerPrefs.SetInt("Pop_UpSource", isPop_UpSourceMute ? 0 : 1);
    //    Audio_Manager.Instance.SetPop_UpSourceMusic(isPop_UpSourceMute);
    //}
    //#endregion

    //#region Drop
    //[SerializeField] private Slider dropSlider;
    //private void LearnDropMusic()
    //{
    //    if (dropSlider != null)
    //    {
    //        dropSlider.value = PlayerPrefs.GetInt("DropMusic", 0);
    //        dropSlider.onValueChanged.AddListener((x) => DropMusic(x));
    //        SetAllSourceMusic(PlayerPrefs.GetInt("DropSource", 0) == 0 ? true : false);
    //    }
    //}
    //private void DropMusic(float volume)
    //{
    //    allMusicMixer.SetFloat("Drop", volume);
    //    PlayerPrefs.SetInt("DropMusic", (int)volume);
    //}
    //// Setting panelinde drop music açma kapama butonlarýnda atandý.
    //public void SetDropSourceMusic(bool isDropSourceMute)
    //{
    //    PlayerPrefs.SetInt("DropSource", isDropSourceMute ? 0 : 1);
    //    Audio_Manager.Instance.SetDropSourceMusic(isDropSourceMute);
    //}
    //#endregion
    #endregion

    #region Vibration
    //public static bool canVibration;
    //[SerializeField] private Toggle vibrationToggle;
    //private void LearnVibration()
    //{
    //    if (canMoveUIToggle != null)
    //    {
    //        canVibration = PlayerPrefs.GetInt("canVibration", canVibration ? 0 : 1) == 0 ? true : false;
    //        vibrationToggle.isOn = canVibration;
    //    }
    //}
    //public void ChangeVibration(bool isActive)
    //{
    //    canVibration = isActive;
    //    PlayerPrefs.SetInt("canVibration", canVibration ? 0 : 1);
    //}
    #endregion

    #region Move UI
    //public static bool canMoveUI;
    //[SerializeField] private Toggle canMoveUIToggle;
    //private void LearnMoveUI()
    //{
    //    if (canMoveUIToggle != null)
    //    {
    //        canMoveUI = PlayerPrefs.GetInt("canMoveUI", canMoveUI ? 0 : 1) == 0 ? true : false;
    //        canMoveUIToggle.isOn = canMoveUI;
    //    }
    //}
    //public void ChangeMoveUI(bool isActive)
    //{
    //    canMoveUI = isActive;
    //    PlayerPrefs.SetInt("canMoveUI", canMoveUI ? 0 : 1);
    //}
    #endregion

    #region Language
    //public static int languageNumber;
    //[SerializeField] private TMP_Dropdown languageDropdown;
    //private void LearnLanguage()
    //{
    //    if (languageDropdown != null)
    //    {
    //        languageNumber = PlayerPrefs.GetInt("languageNumber", languageNumber);
    //        languageDropdown.value = languageNumber;
    //    }
    //}
    //public void ChangeLanguage(int langNumber)
    //{
    //    languageNumber = langNumber;
    //    PlayerPrefs.SetInt("languageNumber", languageNumber);
    //}
    #endregion
}