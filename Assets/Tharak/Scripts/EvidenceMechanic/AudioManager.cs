using UnityEngine;

namespace CaseDesk
{
    // Looping music + one-shot SFX. Assign your clips in the Inspector. Persists across restarts.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        [Header("Clips")]
        public AudioClip musicLoop;
        public AudioClip sfxMenuClick;
        public AudioClip sfxMenuHover;
        public AudioClip sfxPhone;
        public AudioClip sfxFax;
        public AudioClip sfxTip;
        public AudioClip sfxLeak;

        [Header("Volume")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;

        [Header("Max SFX play length (seconds; 0 = play the whole clip)")]
        public float clickMaxSeconds = 0f;
        public float hoverMaxSeconds = 0f;
        public float phoneMaxSeconds = 0f;
        public float faxMaxSeconds = 0f;
        public float tipMaxSeconds = 0f;
        public float leakMaxSeconds = 0f;

        AudioSource music, sfx;
        Coroutine sfxStop;

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);

            music = gameObject.AddComponent<AudioSource>();
            music.loop = true; music.playOnAwake = false; music.volume = musicVolume;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false; sfx.volume = sfxVolume;

            if (musicLoop) { music.clip = musicLoop; music.Play(); }
        }

        public void PlayMusic()
        {
            if (music == null || musicLoop == null) return;
            music.clip = musicLoop; music.volume = musicVolume;
            if (!music.isPlaying) music.Play();
        }

        // Play a clip; if maxSeconds > 0, stop it after that many seconds.
        public void Sfx(AudioClip c, float maxSeconds = 0f)
        {
            if (!c || !sfx) return;
            if (maxSeconds <= 0f) { sfx.PlayOneShot(c, sfxVolume); return; }

            // dedicated playback we can cut short
            if (sfxStop != null) StopCoroutine(sfxStop);
            sfx.clip = c; sfx.volume = sfxVolume; sfx.loop = false; sfx.Play();
            sfxStop = StartCoroutine(StopAfter(maxSeconds));
        }

        System.Collections.IEnumerator StopAfter(float t)
        {
            yield return new WaitForSecondsRealtime(t);
            if (sfx) sfx.Stop();
            sfxStop = null;
        }

        // Play a specific clip (e.g. a phone's own sound). maxSeconds 0 = whole clip.
        public void SfxClip(AudioClip c, float maxSeconds = 0f) { Sfx(c, maxSeconds); }

        public void Click() { Sfx(sfxMenuClick, clickMaxSeconds); }
        public void Hover() { Sfx(sfxMenuHover, hoverMaxSeconds); }
        public void Phone() { Sfx(sfxPhone, phoneMaxSeconds); }
        public void Fax() { Sfx(sfxFax, faxMaxSeconds); }
        public void Tip() { Sfx(sfxTip, tipMaxSeconds); }
        public void Leak() { Sfx(sfxLeak, leakMaxSeconds); }
    }
}
