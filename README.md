# AMASS-Viewer
Unity에서 AMASS 모션캡처 데이터를 읽고 시각화하는 프로젝트

## 개발 환경
- Unity 6000.0.51f1 (LTS)
- Visual Studio Community 2022 (64-bit) 17.14.6
- NumSharp 0.30.0
- NuGetForUnity

## 진행 현황

### 0주차 (3/29 - 4/04)
- AMASS npz 파일 내부 구조 파악 (파이썬)

### 1주차 (4/05 - 4/11)
- Unity 애니메이션 툴 이해 및 테스트

### 2주차 (4/12 - 4/18)
- NuGetForUnity / NumSharp 설치 및 세팅
- NumSharp로 npz 파일 로드 성공
- AMASS poses / trans 키 접근 및 joint 값 콘솔 출력 성공

### 3주차 (4/26 - 5/02)
- SMPL-H 모델 기반 FK(Forward Kinematics) 구현
  - Joint Hierarchy (52개 joint 부모-자식 관계) 정의
  - T-pose offset 정의
  - axis-angle → 회전 행렬 변환 (Rodrigues' rotation formula)
  - 부모 → 자식 순서로 joint 위치 계산
- Unity Scene에 sphere로 joint 시각화 성공
- AMASS 데이터 애니메이션 재생 구현 (mocap framerate 120fps 기준)
- Z-up(AMASS) → Y-up(Unity) 좌표계 변환 처리

### 4주차 (5/03 - 5/09)
- MotionData / MotionList ScriptableObject 구조 설계
- 파일 경로 하드코딩 → StreamingAssets 기반 동적 로드로 교체
- 방향키로 모션 전환 + FK 상태 리셋 흐름 구현
- 엣지 케이스 처리 (빈 파일, 파일 없음, 프레임 수 0)
- 코드 수정 없이 모션 파일만 교체해도 동작하는 구조 완성

### 5주차 (5/10 - 5/16)
- Play / Pause 토글 버튼
- 프레임 단위 이동
- 재생 속도 0.5x / 1x / 2x 전환
- 현재 프레임 번호 + 전체 프레임 수 UI 텍스트 표시

### 6주차 (5/17 - 5/23)
- 드롭다운 UI로 모션 목록 표시 및 즉시 전환
- 타임라인 슬라이더 (드래그로 currentFrame 직접 점프)
- 루프 ON/OFF 토글
- 모션 전환 시 슬라이더 범위 자동 업데이트
- AMASSLoader 공개 API 분리 (LoadMotion / SeekFrame / SetLooping)
- MotionPlayerUI 신규 추가 (UI ↔ 로직 분리)

## 앞으로 계획

### 7주차 (5/24 - 5/30)
- 카메라 시점 프리셋 버튼 (정면 / 측면 / 45도)
- 버그 수정
- (여유 있으면) A–B 구간 반복 재생, 단축키 매핑

### 8주차 (5/31 - 6/06) — 버퍼 주간
- 밀린 작업 마무리 또는 VR 포팅 테스트 시작

### 9주차 (6/07 - 6/13) — 마무리
- 코드 주석 정리 + README 작성
- 시연 영상 촬영 (1~2분)
- VR 포팅 테스트 (가능하면)

## 최종 목표
- AMASS 데이터를 Unity에서 실시간 시각화
- Play / Pause / 속도 조절 / 모션 선택 / 루프 재생 기능 구현
- 시연 영상 + README 완성
- (가능 시) VR 환경에서 동작 확인
