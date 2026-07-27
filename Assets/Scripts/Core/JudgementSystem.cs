using UnityEngine;
using System;

public class JudgementSystem : MonoBehaviour
{
    public static JudgementSystem Instance { get; private set; }

    // 점수 설정
    private const int PERFECT_SCORE = 100;
    private const int GOOD_SCORE = 50;
    private const float COMBO_MULTIPLIER_1 = 1.5f; // 10콤보 이상
    private const float COMBO_MULTIPLIER_2 = 2.0f; // 20콤보 이상

    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;

    public int Score => score;
    public int Combo => combo;
    public int MaxCombo => maxCombo;

    private int perfectCount = 0;
    private int goodCount = 0;
    private int missCount = 0;
    private bool isPenalty = false;   // 페널티 발생 여부
    private string currentStageId = ""; // 현재 플레이 중인 스테이지

    // 판정 결과 이벤트
    public event Action<JudgementType, int, int> OnJudgement;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        StartCoroutine(SubscribeToTouchInput());
    }

    private System.Collections.IEnumerator SubscribeToTouchInput()
    {
        yield return new WaitUntil(() => TouchInputHandler.Instance != null);
        TouchInputHandler.Instance.OnPerfect += HandlePerfect;
        TouchInputHandler.Instance.OnGood += HandleGood;
        TouchInputHandler.Instance.OnMiss += HandleMiss;
    }

    void OnDisable()
    {
        if (TouchInputHandler.Instance != null)
        {
            TouchInputHandler.Instance.OnPerfect -= HandlePerfect;
            TouchInputHandler.Instance.OnGood -= HandleGood;
            TouchInputHandler.Instance.OnMiss -= HandleMiss;
        }
    }

    public void StartStage(string stageId)
    {
        currentStageId = stageId;
        ResetScore();
        Debug.Log($"[JudgementSystem] 스테이지 시작: {stageId}");
    }

    private void HandlePerfect()
    {
        perfectCount++;
        combo++;
        maxCombo = Mathf.Max(maxCombo, combo);
        score += Mathf.RoundToInt(PERFECT_SCORE * GetMultiplier());
        OnJudgement?.Invoke(JudgementType.Perfect, score, combo);
        Debug.Log($"[Judgement] PERFECT | 점수: {score} | 콤보: {combo}");
    }

    private void HandleGood()
    {
        goodCount++;
        combo++;
        maxCombo = Mathf.Max(maxCombo, combo);
        score += Mathf.RoundToInt(GOOD_SCORE * GetMultiplier());
        OnJudgement?.Invoke(JudgementType.Good, score, combo);
        Debug.Log($"[Judgement] GOOD | 점수: {score} | 콤보: {combo}");
    }

    private void HandleMiss()
    {
        missCount++;
        combo = 0;
        OnJudgement?.Invoke(JudgementType.Miss, score, combo);
        Debug.Log($"[Judgement] MISS | 점수: {score} | 콤보: {combo}");
    }

    private float GetMultiplier()
    {
        if (combo >= 20) return COMBO_MULTIPLIER_2;
        else if (combo >= 10) return COMBO_MULTIPLIER_1;
        else return 1.0f;
    }

    public void ResetScore()
    {
        score = 0;
        combo = 0;
        maxCombo = 0;
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;
        isPenalty = false;
    }

    public void EndStage()
    {
        string rank = RankCalculator.Calculate(perfectCount, goodCount, missCount, isPenalty);
        int coinReward = RankCalculator.GetCoinReward(rank);

        DataManager.Instance.UpdateStageRecord(currentStageId, score, maxCombo, rank);

        DataManager.Instance.AddCoins(coinReward);

        FirebaseManager.Instance.SyncToCloud();

        Debug.Log($"[JudgementSystem] 스테이지 종료 | 랭크: {rank} | 주화: +{coinReward}");
        Debug.Log($"Perfect: {perfectCount} | Good: {goodCount} | Miss: {missCount}");
    }
}
