# Refactoring Trace

이 샘플은 프로젝트 리팩터링 문서의 문제 인식과 실제 코드 흐름을 공개 가능한 형태로 압축한 것입니다.

## Scene Lifecycle

Reference: `02_scene_lifecycle.md`

| Original Problem | Public Sample Response |
|---|---|
| 입장과 퇴장 트리거가 Additive Scene 로드와 정리를 직접 처리 | `SceneFlowCoordinator`가 enter/exit 흐름을 단일 진입점으로 가짐 |
| 씬별 정리 코드가 조건문으로 흩어짐 | `AdditiveSceneContext.Cleanup`이 씬 소유 런타임 상태를 정리 |
| 씬 전환 중 상태를 전역 플래그로 판단 | `ActiveScene`과 `IsInFacility`로 현재 전환 상태를 명시 |

## Facility System

Reference: `03_facility_system.md`

| Original Problem | Public Sample Response |
|---|---|
| BaseFacility에 수용량, 대기열, 퇴장, 정령, 시설별 규칙이 집중 | `FacilityBase`는 공통 계약만 담당하고 대기열과 점유를 별도 클래스로 둠 |
| current count와 assigned list가 따로 움직여 불일치 가능 | `FacilityOccupancy`가 assigned herb를 단일 기준으로 관리 |
| Bath와 Sauna의 특수 규칙이 공통 시설 코드에 섞임 | `BathFacility`와 `SaunaFacility`가 각자의 세션과 게이지 규칙만 구현 |
| 정령 배치와 시설 상태 갱신이 서로 강하게 묶임 | `TryAssignElemental`은 공통 시설 API이고, 효과 적용은 각 시설이 override |

## UI Architecture

Reference: `06_ui_architecture.md`

| Original Problem | Public Sample Response |
|---|---|
| UIManager와 UIBase가 생성, 입력, 표시, 폰트, 상태 갱신을 함께 가짐 | 공개 샘플에서는 UI 생성 대신 상태 바인딩만 분리해 보여줌 |
| UI가 시설 내부 상태를 직접 따라가면 갱신 타이밍이 흔들림 | `FacilityStatusBinder`가 `StateChanged`를 구독하고 `FacilityViewModel`만 갱신 |
| 시설별 UI가 증가할수록 UI 코드가 시설 구현을 알아야 함 | UI는 `FacilityStateSnapshot`만 알도록 제한 |

## Why This Is Not A Full Unity Copy

원본 Unity 파일을 그대로 공개하면 미공개 콘텐츠와 팀 작업물이 섞입니다. 이 repo는 코드 리뷰에 필요한 문제 해결 구조만 남겼습니다.

- Unity 전용 타입 제거
- 미공개 씬, 리소스, 데이터 제거
- 시설, 정령, UI, 씬 흐름을 순수 C#으로 재구성
- 커밋 증빙은 `CONTRIBUTIONS.md`에서 공개 가능한 단위로 요약
