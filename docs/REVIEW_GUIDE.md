# Review Guide

이 repo를 보는 사람에게 추천하는 검토 순서입니다. 전체 코드를 오래 읽지 않아도 담당 구현 구간과 사고 흐름이 보이도록 구성했습니다.

## 1. Start With The Demo

```powershell
dotnet run --project samples/Yakchonara.PortfolioSample.Demo
```

출력에서 확인할 흐름:

- `Bath_House` context 등록
- scene loaded 이벤트
- Herb 대기열 등록
- Bath 시설 배정
- UI 모델 갱신
- Elemental 배치와 온도 변화
- Sauna 게이지 갱신
- scene unloaded 이벤트

## 2. Read The Core Classes

| File | What To Check |
|---|---|
| `SceneFlowCoordinator.cs` | Additive Scene 전환 책임을 한 곳으로 모은 방식 |
| `AdditiveSceneContext.cs` | 씬별 시설 초기화와 정리를 분리한 방식 |
| `FacilityBase.cs` | 대기열, 점유, 상태 snapshot, 정령 배치의 공통 계약 |
| `BathFacility.cs` | Bath 전용 온도와 세션 규칙을 공통 시설 흐름 밖으로 뺀 방식 |
| `SaunaFacility.cs` | 같은 시설 계약을 쓰면서 게이지 규칙만 달리 구현한 방식 |
| `FacilityStatusBinder.cs` | 시설 상태 이벤트를 표시 모델로 바꾸는 단방향 UI 갱신 |

## 3. Good Interview Questions

| Question | Expected Direction |
|---|---|
| 왜 모든 것을 Manager 하나에서 처리하지 않았나 | 씬 전환, 시설 런타임 상태, UI 표현은 변경 이유가 다르므로 분리했습니다 |
| 왜 Unity 원본 파일을 그대로 공개하지 않았나 | 팀 작업물과 미공개 콘텐츠 보호가 필요해서 구조만 공개 가능한 C# 샘플로 재구성했습니다 |
| BaseFacility를 더 작게 나눌 수 있나 | 가능하며 이 샘플에서는 `FacilityLineup`과 `FacilityOccupancy`를 먼저 분리했습니다 |
| UI가 시설 객체를 직접 읽으면 더 쉬운가 | 초기 구현은 쉽지만 시설별 상태가 늘어날수록 UI가 내부 구현을 알아야 해서 snapshot 이벤트로 제한했습니다 |
| 정령 scale 복원은 왜 ElementalAgent가 갖나 | 부모 변경의 부작용은 정령 객체 자신의 시각 상태이므로 release 경로에서 함께 복원해야 합니다 |

## 4. What This Sample Does Not Claim

- 실제 게임 전체 빌드를 제공하지 않습니다.
- 원본 프로젝트의 모든 기능을 재현하지 않습니다.
- 성능 수치나 최적화 수치를 주장하지 않습니다.
- 팀 전체 구현을 개인 구현처럼 표현하지 않습니다.

이 repo의 목적은 담당 구간을 공개 가능한 코드로 검토하게 만드는 것입니다.
