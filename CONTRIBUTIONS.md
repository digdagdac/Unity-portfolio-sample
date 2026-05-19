# Contributions

이 문서는 로컬 커밋 기록과 리팩터링 문서를 근거로, 공개 가능한 범위에서 담당 구현을 요약한 것입니다. 원본 repo 주소, 팀명, 미공개 콘텐츠는 포함하지 않았습니다.

## Latest Yakchonara Work

| Local Commit | Theme | Problem | My Implementation | Interview Point |
|---|---|---|---|---|
| `4d6815d` | BathFacility 책임 분리 | 목욕탕 시설에 온도, 세션, 배정, UI 갱신 책임이 몰림 | 시설 공통 흐름은 Base 계층에 남기고 목욕탕 전용 세션과 온도 처리를 별도 컨트롤러로 분리 | 왜 상속만으로 해결하지 않고 컨트롤러를 분리했는가 |
| `30fe7f8` | 온도계 UI | 온도 상태가 게임플레이와 표시 로직 사이에서 직접 연결됨 | 시설 상태 snapshot을 기준으로 UI가 표시값만 갱신하도록 정리 | UI가 모델을 직접 수정하지 않게 만든 이유 |
| `3dfb412` | Sauna and Elemental facility | 사우나와 정령 시설이 Bath 흐름과 비슷하지만 세부 규칙이 달라 중복과 예외가 생김 | 공통 시설 계약을 유지하고 Sauna는 게이지와 세션 규칙만 따로 둠 | 공통화와 시설별 분리의 기준 |
| `5088527` | Player Elemental | 플레이어가 정령을 잡고 시설에 배치하는 상호작용 흐름이 필요함 | 정령 상태를 Idle, Captured, Docked로 나누고 시설 배치 시 상태 전이를 명확히 함 | 입력 처리와 도메인 상태를 분리한 이유 |
| `506335f` | Elemental scale recovery | 정령을 시설에 붙였다가 떼면 부모 변경 뒤 scale이 틀어질 수 있음 | 정령이 기본 scale을 보관하고 release 시 복원하는 흐름으로 정리 | transform 변경 부작용을 어디서 책임질 것인가 |
| `2abb2fe` | Interaction component | 플레이어 상호작용 대상 판정이 여러 객체에 분산됨 | 상호작용 경로를 플레이어 입력과 대상 처리 사이의 중재 흐름으로 정리 | 충돌 판정, 입력, 대상 실행 책임의 경계 |

## Earlier RomancTrain Work

| Local Commit | Theme | Problem | My Implementation | Interview Point |
|---|---|---|---|---|
| `5e54f730` | NewBath facility | 초기 Bath 시설 구조가 이후 확장 요구를 받기 시작함 | 새로운 Bath 시설 흐름을 만들고 이후 리팩터링의 기준점을 확보 | 초기 구현에서 어떤 책임이 커졌는지 설명 |
| `33a61b46`, `8d3b9828` | Temperature options | 온도 표시와 조작 방식에 여러 UI 후보가 필요함 | 온도계 1안과 2안을 실험하며 표시 방식과 게임플레이 피드백을 비교 | UI 실험을 코드 구조와 연결한 방식 |
| `af4f6fba` | Mouse interaction | 플레이어 입력 방식이 키보드 중심에서 마우스 조작으로 확장됨 | 마우스 입력을 통해 대상 선택과 상호작용 흐름을 검증 | 입력 확장 시 기존 코드에 주는 영향 |
| `cfe04318` | Slot position and capacity | 슬롯 위치와 수용량이 하드코딩되면 시설 확장이 어려움 | 위치 계산 함수를 추가하고 슬롯 수용량을 조정 | 데이터와 위치 계산을 분리한 이유 |
| `acc98aab`, `6449afd8` | Player animation test tool | 플레이어 애니메이션 확인이 실제 플레이 흐름에 묶임 | 테스트용 조작 흐름을 만들어 빠른 확인이 가능하게 함 | 개발 도구가 협업 속도에 주는 영향 |

## How This Repo Maps To Those Contributions

- `FacilityBase`, `FacilityLineup`, `FacilityOccupancy`는 시설 공통 책임을 코드로 보여줍니다.
- `BathFacility`, `BathSessionController`, `BathTemperatureController`는 BathFacility 책임 분리의 축약본입니다.
- `SaunaFacility`, `SaunaSession`은 비슷한 시설이라도 세부 규칙을 별도 모델로 유지한 사례입니다.
- `ElementalAgent`, `ElementalActionController`는 정령 잡기, 시설 배치, 해제 흐름을 보여줍니다.
- `FacilityStatusBinder`는 시설 상태 이벤트를 UI 표시 모델로 바꾸는 단방향 갱신 예시입니다.
