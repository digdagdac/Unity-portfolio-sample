# Unity Portfolio Sample

Unity 작업물 중 공개 가능한 구조와 실행 가능한 데모만 분리한 포트폴리오 저장소입니다.

원본 프로젝트의 비공개 기획, 전체 데이터, 서버/네트워크 흐름은 포함하지 않고, 담당 구현 범위를 검토할 수 있는 코드와 씬만 남겼습니다.

## Samples

### 1. Yakchonara Architecture Sample

경영/배치/상태 갱신 구조를 C# 샘플 프로젝트로 분리한 코드 중심 샘플입니다.

Review path:

- `src/Yakchonara.PortfolioSample/SceneFlow`
- `src/Yakchonara.PortfolioSample/Facility`
- `src/Yakchonara.PortfolioSample/Elemental`
- `src/Yakchonara.PortfolioSample/UI`

Run:

```powershell
dotnet build Yakchonara.PortfolioSample.sln
dotnet run --project samples/Yakchonara.PortfolioSample.Demo
```

### 2. Muloro Combat Demo

플레이어 1명과 보스 몬스터 Belphegor 1마리만 남긴 Unity 실행 데모입니다.

Review path:

- `Assets/MuloroCombatDemo/Scenes/PortfolioCombatDemo.unity`
- `Assets/MuloroCombatDemo/Scripts/Portfolio/PortfolioOfflineBoss.cs`
- `Assets/MuloroCombatDemo/Scripts/Portfolio/PortfolioOfflinePlayer.cs`
- `Assets/MuloroCombatDemo/Scripts/Portfolio/PortfolioSinglePlayerBootstrap.cs`

Implemented focus:

- 멀티플레이/Netcode 런타임 제거
- 씬 배치형 싱글 플레이 데모 구성
- Belphegor BT 기반 보스 패턴
- HP 구간별 공격 가중치
- 전방/후방 대시 조건
- StarFinger 후속 애니메이션 데미지 타이밍
- 플레이어 이동/공격/피격 루프

Run in Unity:

1. Open this repository as a Unity project.
2. Open `Assets/MuloroCombatDemo/Scenes/PortfolioCombatDemo.unity`.
3. Press Play.

Validate:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/validate_muloro_combat_demo.ps1
```

## Public Scope

이 저장소는 포트폴리오 공개용입니다. 실제 원본 프로젝트의 전체 에셋, 전체 게임 데이터, 네트워크 로직, 운영용 설정은 포함하지 않습니다.
