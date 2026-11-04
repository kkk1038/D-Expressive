using System.Collections;
using UnityEngine;

/// <summary>Loops the same room theme, adding its prepared damaged variants as mental state falls.</summary>
public class MentalStateBgmController : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
    [SerializeField, Min(0.05f)] private float crossFadeDuration = 0.75f;

    private AudioSource primary;
    private AudioSource secondary;
    private AudioClip normalClip;
    private AudioClip lowClip;
    private AudioClip criticalClip;
    private AudioClip recoveryClip;
    private AudioSource activeSource;
    private AudioClip activeClip;
    private AudioClip requestedClip;
    private Coroutine crossFade;

    private void Awake()
    {
        normalClip = Resources.Load<AudioClip>("Audio/BGM/Melancholic_Room_Normal");
        lowClip = Resources.Load<AudioClip>("Audio/BGM/Melancholic_Room_Mental_50");
        criticalClip = Resources.Load<AudioClip>("Audio/BGM/Melancholic_Room_Mental_10");
        recoveryClip = Resources.Load<AudioClip>("Audio/BGM/Peaceful_Room_Normal");
        if (normalClip == null || lowClip == null || criticalClip == null || recoveryClip == null)
        {
            Debug.LogError("정신력 BGM 파일을 불러오지 못했습니다.");
            enabled = false;
            return;
        }

        primary = CreateSource("BGM Primary");
        secondary = CreateSource("BGM Secondary");
        activeSource = primary;
    }

    private void Start()
    {
        RefreshClip();
    }

    private void Update()
    {
        RefreshClip();
    }

    private AudioSource CreateSource(string sourceName)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }

    private void RefreshClip()
    {
        if (GameStateManager.Instance == null) return;

        GameStateData state = GameStateManager.Instance.CurrentState;
        int mental = state.dayStartMentalState > 0 ? state.dayStartMentalState : state.mentalState;
        if (state.day == 6) mental = 10;
        AudioClip wanted = state.storyFlags.Contains("day7_contact_recovery")
            ? recoveryClip
            : mental <= 10 ? criticalClip : mental <= 50 ? lowClip : normalClip;
        if (wanted == requestedClip) return;

        if (crossFade != null) StopCoroutine(crossFade);
        requestedClip = wanted;
        float fadeDuration = state.storyFlags.Contains("day7_contact_recovery") ? 3f : crossFadeDuration;
        crossFade = StartCoroutine(CrossFadeTo(wanted, fadeDuration));
    }

    private IEnumerator CrossFadeTo(AudioClip nextClip, float duration)
    {
        AudioSource incoming = activeSource == primary ? secondary : primary;
        float preservedTime = activeSource != null && activeSource.isPlaying ? activeSource.time : 0f;
        incoming.clip = nextClip;
        incoming.time = nextClip.length > 0f ? preservedTime % nextClip.length : 0f;
        incoming.volume = 0f;
        incoming.Play();

        float elapsed = 0f;
        float outgoingStartVolume = activeSource != null ? activeSource.volume : 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / duration);
            incoming.volume = Mathf.Lerp(0f, volume, amount);
            if (activeSource != null) activeSource.volume = Mathf.Lerp(outgoingStartVolume, 0f, amount);
            yield return null;
        }

        if (activeSource != null) activeSource.Stop();
        incoming.volume = volume;
        activeSource = incoming;
        activeClip = nextClip;
        crossFade = null;
    }
}
