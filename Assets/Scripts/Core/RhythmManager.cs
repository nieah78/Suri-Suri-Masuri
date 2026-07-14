using UnityEngine;
using System;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    [SerializeField] private float bpm = 120f;

    private double nextBeatTime;
    private double secPerBeat;

    public event Action OnBeat;

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

        secPerBeat = 60.0 / bpm;
        nextBeatTime = AudioSettings.dspTime + secPerBeat;
    }

    void Update()
    {
        // dspTime이 다음 비트 시간을 넘으면 OnBeat 이벤트 발행
        while (AudioSettings.dspTime >= nextBeatTime)
        {
            OnBeat?.Invoke();
            nextBeatTime += secPerBeat;
        }
    }
}