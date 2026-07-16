using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchInputHandler : MonoBehaviour
{
    public static TouchInputHandler Instance { get; private set; }

    // 판정 기준
    private const double PERFECT_THRESHOLD = 0.080; // 80ms
    private const double GOOD_THRESHOLD = 0.160; // 160ms

    // 가장 최근 OnBeat 이벤트 발생 시간
    private double lastBeatTime = 0;

    // 판정 결과
    public event Action OnPerfect;
    public event Action OnGood;
    public event Action OnMiss;

    public enum Judgement {Perfect, Good, Miss}
    

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += HandleFingerDown; // 터치 시작 이벤트 구독
        StartCoroutine(SubscribeToRhythmManager());
    }

    private System.Collections.IEnumerator SubscribeToRhythmManager()
    {
        yield return new WaitUntil(() => RhythmManager.Instance != null);
        RhythmManager.Instance.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        Touch.onFingerDown -= HandleFingerDown;
        EnhancedTouchSupport.Disable();

        if (RhythmManager.Instance != null)
            RhythmManager.Instance.OnBeat -= HandleBeat;
    }

    private void HandleBeat() // 비트 시각
    {
        lastBeatTime = AudioSettings.dspTime;
    }

    private void HandleFingerDown(Finger finger)
    {
        Judge();
    }

    public JudgementResult Judge() // 탭 발생 시 외부에서 호출
    {
        double tapTime = AudioSettings.dspTime;
        double diff = Math.Abs(tapTime - lastBeatTime); // 오차 계산

        JudgementResult result;

        if (diff <= PERFECT_THRESHOLD)
        {
            result = new JudgementResult(JudgementType.Perfect, diff);
            OnPerfect?.Invoke();
        }
        else if (diff <= GOOD_THRESHOLD)
        {
            result = new JudgementResult(JudgementType.Good, diff);
            OnGood?.Invoke();
        }
        else
        {
            result = new JudgementResult(JudgementType.Miss, diff);
            OnMiss?.Invoke();
        }

        Debug.Log($"[TouchInput] {result.Type} | 오차: {result.Diff * 1000:F1}ms");
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        // 테스트용: 스페이스바
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Judge();
        }
    }
}

public enum JudgementType { Perfect, Good, Miss }

public class JudgementResult
{
    public JudgementType Type { get; }
    public double Diff { get; }  // 실제 오차

    public JudgementResult(JudgementType type, double diff)
    {
        Type = type;
        Diff = diff;
    }
}