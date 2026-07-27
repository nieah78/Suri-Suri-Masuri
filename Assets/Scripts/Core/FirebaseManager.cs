using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser currentUser;

    public bool IsInitialized { get; private set; } = false;
    public bool IsOnline { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(InitializeFirebase());
    }

    // ── 초기화 ────────────────────────────────────────

    private IEnumerator InitializeFirebase()
    {
        // Firebase 사용 가능 여부 확인
        var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Result == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            IsInitialized = true;

            Debug.Log("[Firebase] 초기화 완료");

            // 익명 로그인 시도
            StartCoroutine(SignInAnonymously());
        }
        else
        {
            Debug.LogWarning($"[Firebase] 초기화 실패: {checkTask.Result}");
            IsOnline = false;
        }
    }

    private IEnumerator SignInAnonymously()
    {
        var signInTask = auth.SignInAnonymouslyAsync();
        yield return new WaitUntil(() => signInTask.IsCompleted);

        if (signInTask.Exception == null)
        {
            currentUser = signInTask.Result.User;
            IsOnline = true;
            Debug.Log($"[Firebase] 익명 로그인 성공 | UID: {currentUser.UserId}");

            // 로그인 후 클라우드 데이터 동기화
            StartCoroutine(SyncFromCloud());
        }
        else
        {
            Debug.LogWarning($"[Firebase] 로그인 실패 (오프라인 모드): {signInTask.Exception}");
            IsOnline = false;
        }
    }

    // ── 클라우드 → 로컬 동기화 ───────────────────────

    private IEnumerator SyncFromCloud()
    {
        if (!IsOnline) yield break;

        var docRef = db.Collection("players").Document(currentUser.UserId);
        var getTask = docRef.GetSnapshotAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.Exception != null)
        {
            Debug.LogWarning($"[Firebase] 클라우드 불러오기 실패: {getTask.Exception}");
            yield break;
        }

        DocumentSnapshot snapshot = getTask.Result;

        if (snapshot.Exists)
        {
            // 클라우드 데이터가 있으면 로컬과 비교해서 최신 데이터 사용
            int cloudCoins = snapshot.GetValue<int>("coins");
            int localCoins = DataManager.Instance.Data.coins;

            if (cloudCoins > localCoins)
            {
                DataManager.Instance.Data.coins = cloudCoins;
                DataManager.Instance.SaveData();
                Debug.Log($"[Firebase] 클라우드 데이터 적용 | 주화: {cloudCoins}");
            }
            else
            {
                Debug.Log($"[Firebase] 로컬 데이터가 최신 | 주화: {localCoins}");
            }
        }
        else
        {
            // 클라우드에 데이터 없으면 로컬 데이터를 업로드
            yield return StartCoroutine(UploadToCloud());
            Debug.Log("[Firebase] 신규 유저 — 로컬 데이터 업로드");
        }
    }

    // ── 로컬 → 클라우드 업로드 ───────────────────────

    public IEnumerator UploadToCloud()
    {
        if (!IsOnline) yield break;

        PlayerData data = DataManager.Instance.Data;
        var docRef = db.Collection("players").Document(currentUser.UserId);

        // Firestore에 저장할 데이터 구성
        var cloudData = new System.Collections.Generic.Dictionary<string, object>
        {
            { "coins",               data.coins },
            { "selectedCharacterId", data.selectedCharacterId },
            { "unlockedCharacters",  data.unlockedCharacters },
            { "updatedAt",           FieldValue.ServerTimestamp }
        };

        var setTask = docRef.SetAsync(cloudData);
        yield return new WaitUntil(() => setTask.IsCompleted);

        if (setTask.Exception == null)
            Debug.Log("[Firebase] 클라우드 업로드 완료");
        else
            Debug.LogWarning($"[Firebase] 업로드 실패: {setTask.Exception}");
    }

    // 외부에서 업로드 호출 (스테이지 종료 후 등)
    public void SyncToCloud()
    {
        if (IsOnline)
            StartCoroutine(UploadToCloud());
    }
}