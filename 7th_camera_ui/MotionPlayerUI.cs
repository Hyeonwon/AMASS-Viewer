using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모션 선택 드롭다운 + 타임라인 슬라이더 + 루프 토글 + A-B 구간 반복 UI
///
/// [추가된 UI 요소]
///  - setAButton   : Button  — 현재 프레임을 A로 설정
///  - setBButton   : Button  — 현재 프레임을 B로 설정
///  - abLoopToggle : Toggle  — A-B 구간 반복 ON/OFF
///  - abLabel      : TMP_Text — "A: 100  B: 500" 표시
/// </summary>
public class MotionPlayerUI : MonoBehaviour
{
    [Header("로직 참조")]
    public AMASSLoader loader;

    [Header("기본 UI")]
    public TMP_Dropdown motionDropdown;
    public Slider timelineSlider;
    public Toggle loopToggle;
    public TMP_Text frameLabel;
    public TMP_Text motionNameLabel;

    [Header("A-B 구간 UI")]
    public Button setAButton;
    public Button setBButton;
    public Toggle abLoopToggle;
    public TMP_Text abLabel;       // "A: 100  B: 500"

    // ============================================================
    void Start()
    {
        // ── 드롭다운 ───────────────────────────────────────────
        motionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var m in loader.motionList.motions)
            options.Add(m.motionName);
        motionDropdown.AddOptions(options);
        motionDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // ── 슬라이더 ───────────────────────────────────────────
        timelineSlider.wholeNumbers = true;
        timelineSlider.minValue = 0;
        timelineSlider.onValueChanged.AddListener(OnSliderChanged);

        // ── 루프 토글 ──────────────────────────────────────────
        loopToggle.isOn = true;
        loopToggle.onValueChanged.AddListener(OnLoopToggled);

        // ── A-B 버튼 ───────────────────────────────────────────
        setAButton.onClick.AddListener(OnSetA);
        setBButton.onClick.AddListener(OnSetB);

        // ── A-B 토글 ───────────────────────────────────────────
        abLoopToggle.isOn = false;
        abLoopToggle.onValueChanged.AddListener(OnABLoopToggled);

        // ── 로더 이벤트 구독 ───────────────────────────────────
        loader.OnMotionLoaded += RefreshSliderRange;

        if (loader.IsReady)
            RefreshSliderRange();
    }

    // ============================================================
    //  이벤트 핸들러
    // ============================================================

    void OnDropdownChanged(int index)
    {
        loader.LoadMotion(index);
    }

    void OnSliderChanged(float value)
    {
        loader.SeekFrame((int)value);
    }

    void OnLoopToggled(bool isOn)
    {
        loader.SetLooping(isOn);
    }

    void OnSetA()
    {
        loader.SetFrameA(loader.CurrentFrame);
        UpdateABLabel();
    }

    void OnSetB()
    {
        loader.SetFrameB(loader.CurrentFrame);
        UpdateABLabel();
    }

    void OnABLoopToggled(bool isOn)
    {
        loader.SetABLoop(isOn);
    }

    // ============================================================
    //  모션 로드 완료 시 갱신
    // ============================================================
    void RefreshSliderRange()
    {
        timelineSlider.maxValue = loader.TotalFrames - 1;
        timelineSlider.value = 0;

        // A-B 토글 초기화
        abLoopToggle.onValueChanged.RemoveListener(OnABLoopToggled);
        abLoopToggle.isOn = false;
        abLoopToggle.onValueChanged.AddListener(OnABLoopToggled);

        UpdateABLabel();

        if (motionNameLabel != null)
            motionNameLabel.text = loader.motionList.motions[loader.CurrentMotionIndex].motionName;

        motionDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        motionDropdown.value = loader.CurrentMotionIndex;
        motionDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void UpdateABLabel()
    {
        if (abLabel != null)
            abLabel.text = $"A: {loader.FrameA}  B: {loader.FrameB}";
    }

    // ============================================================
    //  매 프레임 동기화
    // ============================================================
    void Update()
    {
        if (!loader.IsReady) return;

        // 슬라이더 자동 이동 — 항상 리스너 제거 후 값 세팅 후 재등록
        timelineSlider.onValueChanged.RemoveListener(OnSliderChanged);
        timelineSlider.value = loader.CurrentFrame;
        timelineSlider.onValueChanged.AddListener(OnSliderChanged);

        if (frameLabel != null)
            frameLabel.text = $"{loader.CurrentFrame} / {loader.TotalFrames - 1}";
    }

    void OnDestroy()
    {
        if (loader != null)
            loader.OnMotionLoaded -= RefreshSliderRange;
    }
}