using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class AudioSyncTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private AudioClip testClip; // 테스트 음악 파일
    [SerializeField] private float bpm = 102f;

    [Header("Metronome Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private bool playMetronome = true;

    private AudioSource audioSource;
    private AudioSource metronomeSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        metronomeSource = gameObject.AddComponent<AudioSource>();
        metronomeSource.clip = clickSound;
        metronomeSource.volume = 0.5f;
    }
    
    void Start()
    {
        RhythmManager.Instance.SetBPM(bpm);

        JudgementSystem.Instance.StartStage("minigame_beat");

        if (testClip != null)
        {
            audioSource.clip = testClip;
            audioSource.Play();
        }

        RhythmManager.Instance.OnBeat += HandleBeat;
    }

    void OnDestroy()
    {
        if (RhythmManager.Instance != null)
            RhythmManager.Instance.OnBeat -= HandleBeat;
    }

    private void HandleBeat()
    {
        if (playMetronome && clickSound != null)
        {
            metronomeSource.Play();
        }

        if (audioSource.isPlaying)
        {
            float musicTime = audioSource.time;
            double dspTime = AudioSettings.dspTime;
            Debug.Log($"[SyncTest] Beat {RhythmManager.Instance.BeatCount} | 음악: {musicTime:F3}s | dsp: {dspTime:F3}s");
        }
    }

    void Update()
    {
        if(Keyboard.current != null & Keyboard.current.rKey.wasPressedThisFrame)
        {
            audioSource.Stop();
            audioSource.Play();
            RhythmManager.Instance.SetBPM(bpm);
            Debug.Log("[SyncTest] 음원 재시작");
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            audioSource.Stop();
            RhythmManager.Instance.SetBPM(0.001f);
            JudgementSystem.Instance.EndStage();
            Debug.Log("[SyncTest] 스테이지 종료 — 음악 및 비트 정지");
        }
    }
}
