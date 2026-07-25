using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private PlayerData playerData;
    private string saveFilePath;

    public PlayerData Data => playerData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 저장 경로 설정
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

        LoadData();
    }

    // ── 저장 / 불러오기 ──────────────────────────────

    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerData, prettyPrint: true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[DataManager] 저장 완료\n경로: {saveFilePath}\n{json}");
    }

    private void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"[DataManager] 불러오기 완료\n{json}");
        }
        else
        {
            // 저장 파일 없으면 기본값으로 초기화
            playerData = new PlayerData();
            SaveData();
            Debug.Log("[DataManager] 새 플레이어 데이터 생성");
        }
    }

    // ── 주화 ─────────────────────────────────────────

    public void AddCoins(int amount)
    {
        playerData.coins += amount;
        SaveData();
        Debug.Log($"[DataManager] 주화 +{amount} | 잔액: {playerData.coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (playerData.coins < amount)
        {
            Debug.Log($"[DataManager] 주화 부족 | 필요: {amount} | 잔액: {playerData.coins}");
            return false;
        }
        playerData.coins -= amount;
        SaveData();
        Debug.Log($"[DataManager] 주화 -{amount} | 잔액: {playerData.coins}");
        return true;
    }

    // ── 캐릭터 ───────────────────────────────────────

    public bool UnlockCharacter(string characterId, int cost)
    {
        if (playerData.unlockedCharacters.Contains(characterId))
        {
            Debug.Log($"[DataManager] 이미 해금된 캐릭터: {characterId}");
            return false;
        }
        if (!SpendCoins(cost)) return false;

        playerData.unlockedCharacters.Add(characterId);
        SaveData();
        Debug.Log($"[DataManager] 캐릭터 해금: {characterId}");
        return true;
    }

    public void SetSelectedCharacter(string characterId)
    {
        if (!playerData.unlockedCharacters.Contains(characterId))
        {
            Debug.Log($"[DataManager] 미해금 캐릭터 선택 불가: {characterId}");
            return;
        }
        playerData.selectedCharacterId = characterId;
        SaveData();
        Debug.Log($"[DataManager] 대표 앵무새 변경: {characterId}");
    }

    // ── 스테이지 기록 ─────────────────────────────────

    public void UpdateStageRecord(string stageId, int score, int combo, string rank)
    {
        StageRecord record = GetOrCreateRecord(stageId);

        // 최고 기록만 갱신
        if (score > record.highScore) record.highScore = score;
        if (combo > record.maxCombo) record.maxCombo = combo;
        if (IsHigherRank(rank, record.bestRank)) record.bestRank = rank;

        // 첫 클리어 보너스 처리
        if (!record.isCleared)
        {
            record.isCleared = true;
            if (!record.isFirstClearRewarded)
            {
                record.isFirstClearRewarded = true;
                AddCoins(20);
                Debug.Log($"[DataManager] 첫 클리어 보너스 +20주화 | 스테이지: {stageId}");
            }
        }

        SaveData();
    }

    private StageRecord GetOrCreateRecord(string stageId)
    {
        StageRecord record = playerData.stageRecords.Find(r => r.stageId == stageId);
        if (record == null)
        {
            record = new StageRecord { stageId = stageId };
            playerData.stageRecords.Add(record);
        }
        return record;
    }

    private bool IsHigherRank(string newRank, string currentRank)
    {
        string[] order = { "S", "A", "B", "C", "" };
        int newIdx = System.Array.IndexOf(order, newRank);
        int curIdx = System.Array.IndexOf(order, currentRank);
        return newIdx < curIdx; // 인덱스가 낮을수록 높은 랭크
    }

    // ── 설정 ─────────────────────────────────────────

    public void SetBGMVolume(float volume)
    {
        playerData.settings.bgmVolume = Mathf.Clamp01(volume);
        SaveData();
    }

    public void SetSFXVolume(float volume)
    {
        playerData.settings.sfxVolume = Mathf.Clamp01(volume);
        SaveData();
    }

    // 앱 종료 시 자동 저장
    void OnApplicationQuit()
    {
        SaveData();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveData();
    }
}