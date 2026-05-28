using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모션 선택 드롭다운 + 타임라인 슬라이더 + 루프 토글 UI
///
/// [씬 셋업]
///  1. Canvas (Screen Space - Overlay) 생성
///  2. 아래 UI 요소들을 만들고 이 컴포넌트의 인스펙터 필드에 연결
///
/// [필수 UI 오브젝트]
///  - motionDropdown  : TMP_Dropdown   — 모션 목록
///  - timelineSlider  : Slider         — 현재 프레임 표시 / 드래그
///  - loopToggle      : Toggle         — 루프 ON/OFF
///  - frameLabel      : TMP_Text       — "120 / 3600" 형태
///
/// [선택 UI 오브젝트]
///  - motionNameLabel : TMP_Text       — 현재 모션 이름 표시
/// </summary>
public class MotionPlayerUI : MonoBehaviour
{
    [Header("로직 참조")]
    public AMASSLoader loader;

    [Header("UI 요소")]
    public TMP_Dropdown motionDropdown;
    public Slider timelineSlider;
    public Toggle loopToggle;
    public TMP_Text frameLabel;          // "120 / 3600"
    public TMP_Text motionNameLabel;     // 선택 사항

    // 슬라이더 드래그 중 자동 갱신을 막기 위한 플래그
    bool isDragging = false;

    // ============================================================
    void Start()
    {
        // ── 드롭다운 초기화 ─────────────────────────────────────
        motionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var m in loader.motionList.motions)
            options.Add(m.motionName);
        motionDropdown.AddOptions(options);

        motionDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // ── 슬라이더 이벤트 ────────────────────────────────────
        // OnPointerDown/Up 이벤트는 EventTrigger 컴포넌트로 연결하거나
        // 아래 Slider.onValueChanged 만으로도 동작합니다.
        timelineSlider.wholeNumbers = true;
        timelineSlider.minValue = 0;
        timelineSlider.onValueChanged.AddListener(OnSliderChanged);

        // ── 루프 토글 ──────────────────────────────────────────
        loopToggle.isOn = true;
        loopToggle.onValueChanged.AddListener(OnLoopToggled);

        // ── 로더 이벤트 구독 ───────────────────────────────────
        loader.OnMotionLoaded += RefreshSliderRange;

        // 로더가 이미 Start에서 LoadMotion(0)을 완료했을 수 있으므로
        // 직접 한 번 갱신
        if (loader.IsReady)
            RefreshSliderRange();
    }

    // ============================================================
    //  이벤트 핸들러
    // ============================================================

    void OnDropdownChanged(int index)
    {
        loader.LoadMotion(index);
        // RefreshSliderRange 는 loader.OnMotionLoaded 이벤트로 자동 호출됨
    }

    void OnSliderChanged(float value)
    {
        // 슬라이더 드래그 → 즉시 프레임 점프
        loader.SeekFrame((int)value);
    }

    void OnLoopToggled(bool isOn)
    {
        loader.SetLooping(isOn);
    }

    // ============================================================
    //  모션 로드 완료 시 슬라이더 범위 갱신
    // ============================================================
    void RefreshSliderRange()
    {
        timelineSlider.maxValue = loader.TotalFrames - 1;
        timelineSlider.value = 0;

        if (motionNameLabel != null)
            motionNameLabel.text = loader.motionList.motions[loader.CurrentMotionIndex].motionName;

        // 드롭다운이 코드로 바뀌는 경우 동기화 (무한 루프 방지를 위해 리스너 일시 제거)
        motionDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        motionDropdown.value = loader.CurrentMotionIndex;
        motionDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    // ============================================================
    //  매 프레임: 슬라이더 + 레이블 동기화
    //  (드래그 중이 아닐 때만 슬라이더를 코드로 옮김)
    // ============================================================
    void Update()
    {
        if (!loader.IsReady) return;

        // 슬라이더가 현재 드래그 중인지 판단
        // Slider 자체의 isPressed는 없으므로 value 변화를 비교하는 간단한 방법 사용:
        // 사용자가 onValueChanged를 통해 SeekFrame을 호출하므로
        // 슬라이더 값과 CurrentFrame이 같으면 "재생 중", 다르면 "드래그 중".
        float sliderFrame = timelineSlider.value;

        // 재생이 자연스럽게 진행될 때만 슬라이더를 따라가게 함
        if (Mathf.Abs(sliderFrame - loader.CurrentFrame) > 1f)
        {
            // 슬라이더가 크게 다르면 사용자가 드래그한 것 → 무시
        }
        else
        {
            // 재생 중 슬라이더 자동 이동 (onValueChanged 발생 방지를 위해 리스너 제거)
            timelineSlider.onValueChanged.RemoveListener(OnSliderChanged);
            timelineSlider.value = loader.CurrentFrame;
            timelineSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // 프레임 레이블 갱신
        if (frameLabel != null)
            frameLabel.text = $"{loader.CurrentFrame} / {loader.TotalFrames - 1}";
    }

    void OnDestroy()
    {
        if (loader != null)
            loader.OnMotionLoaded -= RefreshSliderRange;
    }
}