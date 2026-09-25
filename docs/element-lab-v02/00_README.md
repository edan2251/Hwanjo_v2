# Element Lab v0.2 · 준비와 실행 안내

작성: 2026-09-25. 기존 Element Lab v0.1의 수정 패키지다. 새 게임/새 저장소 생성 지시가 아니다.
이 패키지는 **문서만** 포함한다. 실제 코드·씬·아트·폰트·실행 스크립트는 포함하지 않는다.

## 이번 변경

**사용자 명시 결정:** 차지 사거리 1.75R → 1R. 검흔은 기본 베기로 제자리에서 닿는 공간에 생성.
**이번 실행에 제안하는 방향:** 좌/우/위/아래 4방향 베기. 360도 자유 조준과 한 번에 전 방향을 치는 회전 공격은 만들지 않는다.
4방향은 사용자가 이미 별도로 확정한 과거 결정이 아니라 이번 분석을 반영한 프로토타입 권장안이다. 이 명세로 실행을 지시하면 이번 시험 범위로 사용한다.
추가: 수면 상호작용, 한글 UI·반응 안내, 실험실 유지, 작은 탐험 루프, 검·동작 연결, 패럴랙스·카메라·플랫폼 마무리.

## 넣을 위치

ZIP의 `docs/element-lab-v02`를 저장소 루트에 병합한다.

```text
C:\Users\JEYJEY\Desktop\Hwanjo_v2\
├─ .git\
├─ AGENTS.md
├─ .agents\skills\hwanjo-v2-prototype\SKILL.md
├─ Hwanjo_v2_Test\
├─ references\element-lab-v01\  ← 기존 원본 5장 그대로
└─ docs\
   ├─ PROTOTYPE_SCOPE.md
   ├─ ENVIRONMENT_AUDIT.md
   ├─ HANDOFF.md
   ├─ element-lab-v01\          ← 과거 제작 지시 보존
   └─ element-lab-v02\          ← 이번 패키지
```

기존 `AGENTS.md`, Skill, README, 원본 PNG를 새 파일로 덮어쓰지 않는다.

## 준비 순서

1. 실행 중인 v0.1 게임과 해당 Unity Editor를 저장하고 닫는다. 다른 작업 프로세스를 강제 종료하지 않는다.
2. v0.1 결과가 아직 미커밋이면 Fork에서 코드·리소스·.meta·문서 변경을 검토해 **현 상태 보존용 체크포인트**를 커밋한다. 이는 최종 기획/아트 승인과 다르다. Builds·artifacts·Library·Temp를 새로 추적하지 않는다. 예상 밖 파일은 제외하지 말고 먼저 확인한다.
3. v0.1 결과가 들어 있는 브랜치에서 아래 명령으로 v0.2 브랜치를 만든다. 초기 main에서 만들면 구현이 빠질 수 있다.
4. 이 패키지를 병합한 뒤 Codex에 `04_CODEX_IMPLEMENT.md`를 전달한다. 문서 패키지가 untracked인 것은 정상 입력이다.

```powershell
Set-Location -LiteralPath "C:\Users\JEYJEY\Desktop\Hwanjo_v2"
git status
git branch --show-current
git log -1 --oneline
```

`prototype/element-lab-v01`의 v0.1 체크포인트가 준비되고 게임 파일에 예상 밖 변경이 없는 상태에서:

```powershell
git switch -c prototype/element-lab-v02
```

이미 v0.2 브랜치에서 작업 중이면 새로 만들지 않는다. 같은 이름이 이미 있다는 오류에서 강제 삭제·재생성하지 않는다.

```powershell
codex -C "C:\Users\JEYJEY\Desktop\Hwanjo_v2" --sandbox workspace-write --ask-for-approval on-request
```

기존에 사용하는 모델을 그대로 선택한다. 필요한 Unity 명령 승인은 확인 후 처리한다. Step 0에서 명령별 승인 후 실행이 성공했다는 보고가 있지만, 이번 새 세션의 권한까지 자동 보장하지 않는다.
`danger-full-access`나 승인 우회 옵션을 자동 적용하는 런처는 없다. 무인 완료도 보장하지 않는다.

## Codex 입력창에 붙일 요청

```text
현재 Element Lab v0.1을 이어서 v0.2를 제작해줘.

docs/element-lab-v02/04_CODEX_IMPLEMENT.md를 이번 실행 요청으로 읽고,
같은 폴더의 01~03 명세와 05_QA.md를 따라 A→B→C를 순차 진행해줘.

이번 시험은 기본·차지 모두 1R, 좌/우/위/아래 4방향 베기야.
360도 조준·회전 공격·공격으로 튀어 오르는 이동은 넣지 마.
검흔은 기본 베기 범위 안에 남기고, 아래 베기로 실제 수면을 얼릴 수 있게 해줘.

한글 UI·반응표와 탐험 루프뿐 아니라 C단계의 검/캐릭터 동작,
플랫폼 아트, 배경 패럴랙스, 카메라 댐핑까지 실제 게임에 연결해줘.
기술 테스트만 통과했다고 아트까지 완료로 보고하지 마.

기존 AGENTS.md와 hwanjo-v2-prototype Skill을 읽고 안전 규칙을 유지해.
v0.1 프롬프트 전체나 Step 0을 다시 실행하지 말고 변경분만 적용해.
새 문서·로컬 사용자 변경·원본 이미지·v0.1 빌드를 보존해.
Git commit/push/브랜치 변경은 하지 마.
```

## 읽을 문서

- [조작·반응](01_GAMEPLAY_AND_REACTIONS.md)
- [탐험 구간](02_EXPLORATION.md)
- [아트·카메라](03_ART_AND_CAMERA.md)
- [Codex 마스터 요청](04_CODEX_IMPLEMENT.md)
- [검증 기준](05_QA.md)

정상적으로 진행하면 중간 아이디어 확인을 매번 기다리지 않고 C까지 진행하도록 했다. 실제 권한·사용량·환경 실패는 숨기지 않고 기록하며, 치명적 실패 상태로 다음 단계를 강행하지 않는다.
출력 목표: `Builds/ElementLabV02/HwanjoElementLab.exe` 및 같은 폴더의 전체 데이터.

## 근거와 한계

현재 로컬 v0.1 소스·실행 파일·Skill 본문을 이 패키지 작성자가 직접 검사한 것은 아니다. 제공된 v0.1 명세와 사용자 완료 보고·QA를 기준으로 작성했다. Codex는 실제 구현·튜닝값부터 확인한다.
이 패키지의 정적 검수는 문서·경로·일관성 검사이며 Unity 실행 테스트나 아트 시각 검수가 아니다.

참고한 공개 원문:
- William Pellen 직접 인터뷰: 처음 좌우 공격에서 날아다니는 적에 대응하려 위아래 공격을 추가한 개발 설명.
  https://toschestation.net/on-bugs-and-bouncing-hollow-knights-william-pellen-interview/
- Nintendo Metroid Dread: 자유 조준이 적용되는 동작 설명.
  https://metroid.nintendo.com/dread/samus/
- Unity 2D IK: 목표에 맞춰 뼈대 위치·회전을 계산하는 기능 설명. 이번 패키지가 IK 전환을 요구하는 것은 아님.
  https://docs.unity3d.com/kr/Packages/com.unity.2d.animation%4013.0/manual/2DIK.html
- Unity SpriteRenderer.flipX: 표시 반전과 collider 처리가 다름.
  https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SpriteRenderer-flipX.html
- Hollow Knight 공식 상품 설명: 연결 세계·프레임 애니메이션·패럴랙스·지도 참고. 에셋이나 전체 게임 규모 복제 금지.
  https://store.steampowered.com/app/367520/Hollow_Knight/
- Codex 공식 CLI 안내:
  https://developers.openai.com/codex/cli/reference
