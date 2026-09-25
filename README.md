# Hwanjo_v2

Unity 프로젝트는 [`Hwanjo_v2_Test/`](Hwanjo_v2_Test/)에 있습니다. 저장소 루트는 Git·문서·Skill의 기준 위치입니다.

- [AGENTS.md](AGENTS.md): 저장소 공통 작업 지침.
- [프로토타입 기획 기준](docs/PROTOTYPE_SCOPE.md): 확정·추천·미정사항.
- [환경 확인 결과](docs/ENVIRONMENT_AUDIT.md): 로컬 환경과 실제 검증 기록.
- [인계 문서](docs/HANDOFF.md): 준비 결과, 남은 문제, 다음 작업 시작점.
- [저장소 전용 Skill](.agents/skills/hwanjo-v2-prototype/SKILL.md): v2 구현·수정·회귀 검증 절차.

## Element Lab v0.2 실행

Windows: `Builds/ElementLabV02/HwanjoElementLab.exe`. 같은 폴더의 Data·MonoBleedingEdge·DLL이 함께 필요합니다. Unity 씬은 기존 `Assets/ElementLab/Scenes/ElementLab.unity`입니다.

시작 메뉴에서 **속성 실험실**(네 속성) 또는 **유적 탐험**(불부터 획득)을 선택합니다. `A/D` 이동, `Space` 점프, `Shift` 대시, `J`를 짧게 눌렀다 놓으면 단타, 지상에서 0.5초 이상 누른 뒤 놓으면 차지입니다. 기본·차지 모두 1R이며 `W/↑+J` 위, `S/↓+J` 아래, 나머지는 바라보는 좌우 방향입니다. 공중에서는 짧은 단타만 가능합니다.

`1/2/3/4` 불/물/바람/얼음, `E` 출구·권능·체크포인트, `M` 방문 지도, `R` 현재 방 재시작 확인, `Esc` 메뉴입니다. 권능·지도·발견·지름길은 실행 중 유지되며 프로그램 종료 후 저장되지 않습니다. 전체 진행 초기화는 메뉴의 **탐험 처음부터**로 구분됩니다.

`F1` 반응 기록/판정, `F2` 실험실 더미, `F3` 타격감, `F4` 실제 검흔 반응표, `F5` 카메라 완화 비교, `F6` 속성 효과 비교, `F9` 화면 저장. 물가의 노출된 수면은 아래 얼음 베기로 얼리고 아래 불 베기로 해동할 수 있습니다. 검흔은 가까운 세계 좌표에 2초 남으며, 물 검흔→얼음만 지지면이 있을 때 임시 발판이 됩니다.

- [v0.2 실제 검증과 한계](docs/V02_VALIDATION.md)
- [본선·비밀방 동선](docs/V02_LEVEL_DESIGN.md)
- [변경 기록](docs/V02_CHANGELOG.md)

## 보존된 Element Lab v0.1 실행

Windows: `Builds/ElementLabV01/HwanjoElementLab.exe`. 같은 폴더의 Data·MonoBleedingEdge·DLL도 함께 필요합니다. 빌드 폴더는 Git 제외 대상입니다.

v0.1은 보존된 EXE로 실행합니다. 현재 Unity 6000.3.23f1의 `Hwanjo_v2_Test/Assets/ElementLab/Scenes/ElementLab.unity`는 v0.2 구현을 사용합니다. 기존 SampleScene은 별도로 보존합니다.

`A/D` 이동, `Space` 점프, `Shift` 대시, `J` 짧게 눌렀다 놓기=단타, `J` 0.5초 이상 누른 뒤 놓기=차지, `1/2/3/4` 불/물/바람/얼음, `R` 전체 초기화, `Esc` 일시정지/종료 메뉴. 차지 뒤 다른 속성을 선택하고 제자리 단타로 검흔을 다시 벨 수 있습니다.

`F1` 반응 기록/판정 범위, `F2` 더미 편집, `F3` 타격감 비교, `F9` 실제 화면 PNG. 아래 구역 버튼으로 실험 장소를 이동합니다. 더미 편집 중 월드가 멈추며 Apply/Reset Target/Default Preset은 서로 다른 기능입니다.

- [검증 결과와 알려진 한계](docs/PROTOTYPE_VALIDATION.md)
- [아트 출처·프레임·가공 기록](docs/ART_ASSET_MANIFEST.md)
- [실제 임시값](docs/PROTOTYPE_SCOPE.md)
