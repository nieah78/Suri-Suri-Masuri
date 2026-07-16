using UnityEngine;
using System;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    [SerializeField] private float bpm = 120f;

    private double nextBeatTime;
    private double secPerBeat;
    private int beatCount = 0;

    public event Action OnBeat;
    public event Action<int> OnBeatWithCount;

    public float BPM => bpm;
    public int BeatCount => beatCount;
    public double SecPerBeat => secPerBeat;

    void Awake()
    {
        // 씬에 하나만 존재하도록 싱글톤 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeTiming();
    }

    void Update()
    {
        // dspTime이 다음 비트 시간을 넘으면 OnBeat 이벤트 발행
        while (AudioSettings.dspTime >= nextBeatTime)
        {
            beatCount++;
            OnBeat?.Invoke();
            OnBeatWithCount?.Invoke(beatCount);
            nextBeatTime += secPerBeat;

            Debug.Log($"[RhythmManager] Beat {beatCount} | dspTime: {AudioSettings.dspTime:F3}");
        }
    }

    // BPM 런타임 변경 (난이도 변화, 곡 전환)
    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
        InitializeTiming();
        Debug.Log($"[RhythmManager] BPM 변경: {bpm}");
    }

    // 타이밍 초기화 (Awake, BPM 변경)
    private void InitializeTiming()
    {
        secPerBeat = 60.0 / bpm;
        nextBeatTime = AudioSettings.dspTime + secPerBeat;
        beatCount = 0;
    }
}