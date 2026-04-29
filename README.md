# AMASS-Viewer
Unity에서 AMASS 모션캡처 데이터를 읽고 시각화하는 프로젝트

## 개발 환경
- Unity 6000.0.51f1 (LTS)
- Visual Studio Community 2022 (64-bit) 17.14.6
- NumSharp 0.30.0
- NuGetForUnity

## 진행 현황
### 1주차 (3/29 - 4/04)
- AMASS npz 파일 내부 구조 파악 (파이썬)

### 2주차 (4/05 - 4/11)
- Unity 애니메이션 툴 이해 및 테스트

### 3주차 (4/12 - 4/18)
- NuGetForUnity / NumSharp 설치 및 세팅
- NumSharp로 npz 파일 로드 성공
- AMASS poses / trans 키 접근 및 joint 값 콘솔 출력 성공

### 4주차 (4/26 - 5/02)
- SMPL-H 모델 기반 FK(Forward Kinematics) 구현
  - Joint Hierarchy (52개 joint 부모-자식 관계) 정의
  - T-pose offset 정의
  - axis-angle → 회전 행렬 변환 (Rodrigues' rotation formula)
  - 부모 → 자식 순서로 joint 위치 계산
- Unity Scene에 sphere로 joint 시각화 성공
- AMASS 데이터 애니메이션 재생 구현 (mocap framerate 120fps 기준)
- Z-up(AMASS) → Y-up(Unity) 좌표계 변환 처리

## 최종 목표
- AMASS 데이터를 Unity에서 실시간 시각화
- Play / Pause / 속도 조절 / 모션 선택 / 루프 재생 기능 구현
- (가능 시) VR 환경에서 동작 확인
