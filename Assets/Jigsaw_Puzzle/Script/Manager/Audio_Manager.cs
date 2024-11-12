using UnityEngine;
using System.Collections;

public class Audio_Manager : Singletion<Audio_Manager>
{
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip gameClip;
    [SerializeField] private AudioClip goldChance;
    [SerializeField] private AudioClip puzzleFixed;
    [SerializeField] private AudioClip puzzlePieceMixed;
    [SerializeField] private AudioClip puzzlePieceRightPosition;
    [SerializeField] private AudioClip puzzleStartPanelOpened;
    //[SerializeField] private AudioClip roomClip;
    //[SerializeField] private AudioClip offerClip;
    [Space]
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource uISource;
    [SerializeField] private AudioSource effectSource;
    //[SerializeField] private AudioSource gunSource;
    //[SerializeField] private AudioSource explosionSource;
    //[SerializeField] private AudioSource pop_UpSource;
    //[SerializeField] private AudioSource dropSource;

    #region Background Fonksiyonlarý
    //public void OfferSound()
    //{
    //    StartCoroutine(ChanceMusic(offerClip));
    //}
    public void MenuSound()
    {
        StartCoroutine(ChanceMusic(menuClip));
    }
    public void GameSound()
    {
        StartCoroutine(ChanceMusic(gameClip));
    }
    //public void RoomSound()
    //{
    //    StartCoroutine(ChanceMusic(roomClip));
    //}
    IEnumerator ChanceMusic(AudioClip newClip)
    {
        bool chancedMusic = false;
        int chancedDirection = -1;
        while (!chancedMusic)
        {
            yield return null;
            if (chancedDirection == -1)
            {
                if (backgroundSource.volume > 0)
                {
                    backgroundSource.volume += chancedDirection * Time.deltaTime;
                }
                else
                {
                    chancedDirection = 1;
                    backgroundSource.clip = newClip;
                    backgroundSource.Play();
                }
            }
            else
            {
                if (backgroundSource.volume < 1)
                {
                    backgroundSource.volume += chancedDirection * Time.deltaTime;
                }
                else
                {
                    chancedMusic = true;
                }
            }
        }
    }
    #endregion

    #region Play Fonksiyonlarý
    public void PlayGoldChance()
    {
        effectSource.clip = goldChance;
        effectSource.Play();
    }
    public void PlayPuzzleFixed()
    {
        effectSource.clip = puzzleFixed;
        effectSource.Play();
    }
    public void PlayPuzzlePieceRightPlace()
    {
        effectSource.clip = puzzlePieceRightPosition;
        effectSource.Play();
    }
    public void PlayPuzzlePieceMixed()
    {
        effectSource.clip = puzzlePieceMixed;
        effectSource.Play();
    }
    public void PlayPuzzleStartPanelOpened()
    {
        uISource.Play();
    }
    //public void PlayGunSourceMusic(AudioClip audioClip)
    //{
    //    gunSource.PlayOneShot(audioClip);
    //}
    //public void PlayExplosionSourceMusic(AudioClip audioClip)
    //{
    //    explosionSource.PlayOneShot(audioClip);
    //}
    //public void PlayPop_UpSourceMusic()
    //{
    //    pop_UpSource.Play();
    //}
    //public void PlayDropSourceMusic()
    //{
    //    dropSource.Play();
    //}
    #endregion

    #region Set Fonksiyonlarý
    public void SetAllSourceMusic(bool isAllMusicSourceMute)
    {
        backgroundSource.mute = isAllMusicSourceMute;
        uISource.mute = isAllMusicSourceMute;
        effectSource.mute = isAllMusicSourceMute;
        //gunSource.mute = isAllMusicSourceMute;
        //explosionSource.mute = isAllMusicSourceMute;
        //pop_UpSource.mute = isAllMusicSourceMute;
        //dropSource.mute = isAllMusicSourceMute;
    }
    public void SetBackgroundSourcMusic(bool isBackgroundSourceMute)
    {
        backgroundSource.mute = isBackgroundSourceMute;
    }
    public void SetUISourceMusic(bool isUISourceMute)
    {
        uISource.mute = isUISourceMute;
    }
    public void SetEffectSourceMusic(bool isEffectSourceMute)
    {
        effectSource.mute = isEffectSourceMute;
    }
    //public void SetGunSourcelMusic(bool isGunSourceMute)
    //{
    //    gunSource.mute = isGunSourceMute;
    //}
    //public void SetExplosionSourceMusic(bool isExplosionSourceMute)
    //{
    //    explosionSource.mute = isExplosionSourceMute;
    //}
    //public void SetPop_UpSourceMusic(bool isPop_UpSourceMute)
    //{
    //    pop_UpSource.mute = isPop_UpSourceMute;
    //}
    //public void SetDropSourceMusic(bool isDropSourceMute)
    //{
    //    dropSource.mute = isDropSourceMute;
    //}
    #endregion
}