# 환조 v2 · Element Lab v0.3 제작 패키지

**목표: 방 전환형 탐험을 하나의 연속된 소형 월드로 재구성한다. 레벨디자인, 수직 이동에 필요한 최소 벽 행동, 추가 환경 아트 제작·통합이 이번 범위다.**

이 패키지는 기존 v0.2 제작 문서와 사용자의 완료 보고·최신 피드백을 바탕으로 만든 작업 지시서다. 작성자가 최신 로컬 게임 코드를 직접 실행하거나 실제 맵을 검증한 결과는 아니다.

## 사용 순서

1. 현재 v0.2 결과를 Fork에서 검토해 로컬 체크포인트 커밋을 남긴다. 이는 최종 기획·아트 승인이 아니라 복구 기준점이다. Builds/artifacts/Library/Temp를 새로 추적하지 않는다.
2. **v0.2 결과가 들어 있는 현재 브랜치**에서 `prototype/element-lab-v03`을 만든다. 초기 main으로 돌아가서 만들지 않는다. 이미 해당 브랜치라면 생성 명령은 반복하지 않는다.
3. 압축의 `docs` 폴더를 Git 루트에 합친다. `Hwanjo_v2_Test/Assets` 안에 넣지 않는다.
4. Unity를 저장·정상 종료한 뒤 같은 프로젝트의 Codex 세션에서 아래 요청을 전달한다. 새로 열 때의 명령도 아래에 있다.
5. 제작 결과·새 빌드·로그를 확인한다. 자동 commit/push는 이번 요청에 포함하지 않는다.

기대 경로:

```text
C:\Users\JEYJEY\Desktop\Hwanjo_v2\
├─ AGENTS.md
├─ .agents\skills\hwanjo-v2-prototype\SKILL.md
├─ Hwanjo_v2_Test\
├─ references\element-lab-v01\       기존 원본 5장 보존
└─ docs\element-lab-v03\             이번 폴더
```

PowerShell에서 확인·브랜치 생성:

```powershell
Set-Location -LiteralPath "C:\Users\JEYJEY\Desktop\Hwanjo_v2"
git status
git branch --show-current
git log -1 --oneline
# v0.2 체크포인트를 확인한 다음, 아직 v03 브랜치를 만들지 않았을 때만 실행:
git switch -c prototype/element-lab-v03
```

Codex를 새로 시작할 때:

```powershell
codex -C "C:\Users\JEYJEY\Desktop\Hwanjo_v2" --sandbox workspace-write --ask-for-approval on-request
```

Codex 입력창에 전달:

```text
현재 Element Lab v0.2를 이어서 v0.3을 제작해줘.

docs/element-lab-v03/01_CODEX_IMPLEMENT.md를 이번 실행 요청으로 읽고,
02_LEVEL_AND_MOVEMENT.md, 03_ART_AND_CAMERA.md, 04_QA_AND_HANDOFF.md를 따라
레벨 재구성 → 실제 이동 검증 → 환경 아트 통합 → 최종 검증·Windows 빌드까지 진행해줘.

핵심은 한 탐험 씬의 공통 좌표에서 실제로 걸어 다니는 연속 맵이야.
방 이동 텔레포트·페이드·조작 정지·경계마다 초기화하는 방식은 제거해.
좌우와 상하가 연결된 동선, 재방문·지름길, 숨겨진 공간 2곳을 만들어줘.
벽 행동은 벽 슬라이드와 벽 점프까지만 추가하고,
기본 전투·속성·검흔 규칙은 유지해.

기존 동양풍 도트와 주인공은 보존하고,
Ori는 자연형 지형과 공간의 깊이, Hollow Knight는 연결·재방문 구조의 참고로 사용해.
새 지형·벽면·제단·지름길·비밀 입구 아트와 벽 행동 표현을 실제 게임에 적용해.
문서나 회색 블록만 만들고 끝내지 마.

AGENTS.md와 기존 hwanjo-v2-prototype Skill을 사용해.
v0.2 전체 제작, Step 0, 이전 야간 실행기는 다시 실행하지 마.
원본·기존 빌드·사용자 변경을 보존하고 Git commit/push/브랜치 변경은 하지 마.
```

## 패키지 구성

| 문서 | 역할 |
|---|---|
| [01_CODEX_IMPLEMENT.md](01_CODEX_IMPLEMENT.md) | 실행 요청, 변경 범위, 진행 순서, 보호 규칙 |
| [02_LEVEL_AND_MOVEMENT.md](02_LEVEL_AND_MOVEMENT.md) | 연속 월드·순환 동선·벽 행동·복구·지도 |
| [03_ART_AND_CAMERA.md](03_ART_AND_CAMERA.md) | 자연형 플랫폼·벽면·랜드마크·패럴랙스·벽 동작 |
| [04_QA_AND_HANDOFF.md](04_QA_AND_HANDOFF.md) | 실제 통과·관문 우회·상태 유지·시각 검증·보고 |
| PACKAGE_REVIEW.json | 이 패키지의 파일·링크·문자 형식 등 정적 점검. 게임 검증이 아님 |

추가 의존 프로그램·자동 커밋 런처·새 Skill 설치 파일은 없다. 이번 문서가 untracked로 보이는 것은 정상이다.
기존 준비 문서와 Skill을 덮어쓰지 않는다. 경로가 다른 PC라면 실제 Git 루트를 확인해 사용한다.

## 이번 작업의 임시 선택

- 하나의 연결된 탐험 씬, 주요 랜드마크 구역 6개와 숨겨진 공간 2곳. 구역은 방 전환 단위가 아니다.
- 벽 슬라이드·벽 점프는 이번 시험에서 시작부터 사용. 별도 벽 붙잡기 버튼·자유 등반·스태미나는 추가하지 않는다.
- 불→물→얼음→물길 재방문→바람→성소 지름길이라는 v0.2 시연 흐름은 최대한 유지하되 실제 지형을 재배치한다.
- 지역 초기화는 사용자 명시 입력 때만. 경계 통과는 진행·상태를 초기화하지 않는다.
- 실행 중 진행 유지까지만 제공한다. 종료 후 이어하기는 새로 만들지 않는다.
- 정확한 지형 좌표·벽 행동 수치·아트 조각 수는 실제 이동 측정 후 좁게 정하고 문서에 기록한다.

결과 목표: `Builds/ElementLabV03/HwanjoElementLab.exe`와 필요한 데이터 폴더 전체.
로그 목표: `artifacts/element-lab-v03/<run-id>/`.
작업 보고: `docs/V03_LEVEL_DESIGN.md`, `docs/V03_VALIDATION.md`, 기존 `docs/HANDOFF.md`.

## 승인과 중단

이 명령은 대화형이며 필요한 Unity 실행 권한은 승인 요청이 나올 수 있다. `never`나 전체 접근 모드를 몰래 켜지 않는다.
정상 구현 단계마다 확인 질문을 반복하지 않되, 권한·계정·사용량·사용자 파일 충돌은 숨기지 않고 보고한다.
출력 색깔만으로 실패라고 판단하지 않고 실제 로그와 종료 코드를 기록한다. 재개 시 완료 부분을 조사한 뒤 남은 작업만 수행한다.
