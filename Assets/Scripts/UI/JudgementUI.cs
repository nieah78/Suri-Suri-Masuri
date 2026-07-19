using UnityEngine;
using TMPro;
using System.Collections;

public class JudgementUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI judgementText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 0.8f; // 판정 결과 표시 시간
    [SerializeField] private float punchScale = 1.3f;

    private Coroutine judgementCoroutine;

    void OnEnable()
    {
        StartCoroutine(SubscribeToJudgementSystem());
    }

    private System.Collections.IEnumerator SubscribeToJudgementSystem()
    {
        yield return new WaitUntil(() => JudgementSystem.Instance != null);
        JudgementSystem.Instance.OnJudgement += HandleJudgement;
    }

    private void HandleJudgement(JudgementType type, int score, int combo)
    {
        scoreText.text = score.ToString("N0");
        comboText.text = combo > 1 ? $"x{combo} COMBO" : "";

        if (judgementCoroutine != null)
            StopCoroutine(judgementCoroutine);
        judgementCoroutine = StartCoroutine(ShowJudgement(type));
    }

    private IEnumerator ShowJudgement(JudgementType type)
    {
        // 판정별 텍스트·색상 설정
        switch (type)
        {
            case JudgementType.Perfect:
                judgementText.text = "PERFECT";
                judgementText.color = new Color(1f, 0.85f, 0f);   // 골드
                break;
            case JudgementType.Good:
                judgementText.text = "GOOD";
                judgementText.color = new Color(0.3f, 0.7f, 1f);  // 하늘색
                break;
            case JudgementType.Miss:
                judgementText.text = "MISS";
                judgementText.color = new Color(1f, 0.3f, 0.3f);  // 빨강
                break;
        }

        // 펀치 애니메이션
        judgementText.gameObject.SetActive(true);
        judgementText.transform.localScale = Vector3.one * punchScale;

        float elapsed = 0f;
        float punchDuration = 0.1f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            float scale = Mathf.Lerp(punchScale, 1f, t);
            judgementText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        judgementText.transform.localScale = Vector3.one;

        // 잠시 표시 후 페이드 아웃
        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        float fadeDuration = 0.2f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            Color c = judgementText.color;
            judgementText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        judgementText.gameObject.SetActive(false);
    }
}
