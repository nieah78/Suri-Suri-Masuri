using System;
using System.Collections.Generic;

// 전체 플레이어 데이터 구조
[Serializable]
public class PlayerData
{
    public int coins = 0;                              // 보유 주화
    public string selectedCharacterId = "koko";        // 대표 앵무새 ID
    public List<string> unlockedCharacters             // 해금된 캐릭터 목록
        = new List<string> { "koko" };
    public List<StageRecord> stageRecords              // 스테이지별 기록
        = new List<StageRecord>();
    public SettingsData settings = new SettingsData(); // 설정값
}

// 미니게임별 클리어 기록
[Serializable]
public class StageRecord
{
    public string stageId;       // ex: "minigame_beat", "minigame_scratch"
    public int highScore = 0;    // 최고 점수
    public string bestRank = ""; // 최고 랭크 (S/A/B/C)
    public int maxCombo = 0;     // 최대 콤보
    public bool isCleared = false;         // 클리어 여부
    public bool isFirstClearRewarded = false; // 첫 클리어 보너스 지급 여부
}

// 설정값
[Serializable]
public class SettingsData
{
    public float bgmVolume = 1.0f;  // BGM 볼륨
    public float sfxVolume = 1.0f;  // SFX 볼륨
}