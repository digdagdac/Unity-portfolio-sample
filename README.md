# Yakchonara Portfolio Sample

Unity 협업 프로젝트에서 맡았던 시설, 정령, 씬 전환, UI 상태 갱신 구조를 공개 검토용으로 재구성한 순수 C# 샘플입니다.

이 repo는 원본 Unity 프로젝트가 아닙니다. 미공개 기획, 팀 정보, 씬 데이터, 이미지, 사운드, 테이블, 빌드 산출물은 포함하지 않았습니다. 대신 면접관이 코드만 보고도 구조적 판단과 문제 해결 과정을 검토할 수 있도록 핵심 흐름을 작은 샘플로 분리했습니다.

## What To Review First

1. `src/Yakchonara.PortfolioSample/SceneFlow`
   - Additive Scene 입장과 퇴장을 `SceneFlowCoordinator`가 한 곳에서 관리합니다.
   - 씬별 초기화와 정리는 `AdditiveSceneContext`가 담당합니다.

2. `src/Yakchonara.PortfolioSample/Facility`
   - `FacilityBase`는 공통 상태, 대기열, 점유, 정령 배치를 관리합니다.
   - `BathFacility`와 `SaunaFacility`는 온도, 게이지, 세션처럼 시설별 규칙만 담당합니다.

3. `src/Yakchonara.PortfolioSample/Elemental`
   - `ElementalAgent`는 잡기, 시설 배치, 해제, scale 복원 흐름을 명시적으로 가집니다.
   - `ElementalActionController`는 플레이어 입력과 시설 할당 사이의 얇은 중재자 역할을 합니다.

4. `src/Yakchonara.PortfolioSample/UI`
   - `FacilityStatusBinder`는 시설 상태 이벤트를 구독해 표시용 모델만 갱신합니다.
   - UI가 시설 내부 상태를 직접 뒤지는 흐름을 피하도록 구성했습니다.

## Problem Solving Focus

| Problem | Approach | Sample Code |
|---|---|---|
| 씬 입장, 퇴장, 정리 흐름이 트리거와 매니저에 흩어짐 | `SceneFlowCoordinator`와 `AdditiveSceneContext`로 씬 생명주기 분리 | `SceneFlow` |
| 시설 공통 로직과 시설별 규칙이 같은 클래스에 섞임 | `FacilityBase`는 공통 계약만 갖고 Bath/Sauna는 세부 규칙만 구현 | `Facility` |
| UI가 시설 상태를 직접 읽어 갱신 타이밍이 불안정함 | `StateChanged` 이벤트와 `FacilityStateSnapshot`으로 단방향 갱신 | `UI` |
| 정령을 부모 오브젝트에 붙였다가 뗄 때 상태 복원이 누락될 수 있음 | `ElementalAgent`가 dock/release 상태와 scale 복원을 소유 | `Elemental` |

## Run

```powershell
dotnet build Yakchonara.PortfolioSample.sln
dotnet run --project samples/Yakchonara.PortfolioSample.Demo
```

데모는 다음 흐름을 출력합니다.

1. Bath scene context 등록
2. Additive Scene 입장
3. Herb 대기열 등록과 시설 배정
4. Bath 상태 변경과 UI binder 갱신
5. Elemental 배치와 온도 변화
6. Sauna 게이지 갱신
7. Bath 세션 완료
8. Scene 퇴장과 시설 정리

## Documents

- `CONTRIBUTIONS.md`: 커밋 기록 기반 담당 구현 요약
- `docs/ARCHITECTURE.md`: 주요 흐름 다이어그램
- `docs/REFACTORING_TRACE.md`: 리팩터링 문서와 샘플 코드 연결
- `docs/REVIEW_GUIDE.md`: 면접관이 보기 좋은 코드 리뷰 순서

## Public Scope

이 샘플은 공개 포트폴리오를 위한 재구성본입니다. 원본 프로젝트의 미공개 콘텐츠를 그대로 옮기지 않았고, Unity 전용 타입도 제거했습니다. 그래서 실행 결과는 게임 플레이가 아니라 구조 검증용 시뮬레이션입니다.
