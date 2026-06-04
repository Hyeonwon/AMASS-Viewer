using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카메라 시점 프리셋 버튼 3개
///
/// [씬 셋업]
///  1. Main Camera에 이 컴포넌트를 추가
///  2. Canvas에 Button 3개 만들고 인스펙터에 연결
///     - frontButton  : 정면
///     - sideButton   : 측면
///     - quarterButton: 45도
/// </summary>
public class CameraPreset : MonoBehaviour
{
    [Header("UI 버튼")]
    public Button frontButton;
    public Button sideButton;
    public Button quarterButton;

    [Header("타겟 (스켈레톤 중심)")]
    public Transform target; // 비워두면 원점(0,0,0) 기준

    [Header("카메라 거리")]
    public float distance = 3f;

    // ── 프리셋 정의 ───────────────────────────────────────────
    // (position offset, rotation)
    static readonly Vector3 FrontPos = new Vector3(0f, 1f, -1f);
    static readonly Vector3 SidePos = new Vector3(1f, 1f, 0f);
    static readonly Vector3 QuarterPos = new Vector3(1f, 1f, -1f);

    static readonly Quaternion FrontRot = Quaternion.Euler(0f, 0f, 0f);
    static readonly Quaternion SideRot = Quaternion.Euler(0f, -90f, 0f);
    static readonly Quaternion QuarterRot = Quaternion.Euler(15f, -45f, 0f);

    // ── 스무스 이동용 ─────────────────────────────────────────
    Vector3 targetPosition;
    Quaternion targetRotation;
    bool isMoving = false;
    float moveSpeed = 5f;

    // ============================================================
    void Start()
    {
        // 초기 위치를 정면으로
        ApplyPreset(FrontPos, FrontRot, instant: true);

        frontButton.onClick.AddListener(() => ApplyPreset(FrontPos, FrontRot));
        sideButton.onClick.AddListener(() => ApplyPreset(SidePos, SideRot));
        quarterButton.onClick.AddListener(() => ApplyPreset(QuarterPos, QuarterRot));
    }

    void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);

        // 충분히 가까워지면 정지
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            isMoving = false;
        }
    }

    // ============================================================
    void ApplyPreset(Vector3 posOffset, Quaternion rot, bool instant = false)
    {
        Vector3 center = target != null ? target.position : Vector3.zero;

        targetPosition = center + posOffset.normalized * distance + Vector3.up * posOffset.y;
        targetRotation = rot;

        if (instant)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
    }
}