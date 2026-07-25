public static class RankCalculator
{
    public static string Calculate(int perfectCount, int goodCount, int missCount, bool isPenalty)
    {
        int total = perfectCount + goodCount + missCount;
        if (total == 0) return "C";

        float perfectRatio = (float)perfectCount / total;

        // S: 전 구간 Perfect, 페널티 0회
        // A: 90% 이상 Perfect
        // B: 70% 이상 Good 이상
        // C: 70% 미만

        if (perfectRatio >= 1.0f && !isPenalty) return "S";
        if (perfectRatio >= 0.9f) return "A";

        float clearRatio = (float)(perfectCount + goodCount) / total;
        if (clearRatio >= 0.7f) return "B";

        return "C";
    }

    // 랭크별 주화 지급량
    public static int GetCoinReward(string rank)
    {
        switch (rank)
        {
            case "S": return 50;
            case "A": return 30;
            case "B": return 15;
            default:  return 5;
        }
    }
}