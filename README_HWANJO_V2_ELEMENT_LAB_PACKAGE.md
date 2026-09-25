# 환조 v2 — 아트·프로토타입 제작 패키지

이 패키지는 세 가지를 제공한다.

1. 제공된 원본 5장을 역할별로 나눈 아트 디렉션.
2. 새 `Hwanjo_v2_Test`에서 작은 속성 실험실을 구현할 Codex 요청과 임시 명세.
3. 원본 레퍼런스 폴더와 배치/실행 안내.

게임 코드나 완성된 새 스프라이트를 이미 제작한 패키지는 아니다. 자동 PowerShell 런처도 포함하지 않는다.

## 폴더 배치

압축을 푼 폴더의 **docs와 references**를 아래 Git 루트에 합쳐 넣는다.

`C:\Users\JEYJEY\Desktop\Hwanjo_v2`

```text
Hwanjo_v2/
  .git/
  Hwanjo_v2_Test/                   # 기존 Unity 프로젝트, 건드리지 않음
  AGENTS.md                        # Step 0에서 준비, 패키지에는 없음
  .agents/skills/...               # Step 0에서 준비, 패키지에는 없음
  docs/
    ENVIRONMENT_AUDIT.md           # Step 0 결과, 패키지에는 없음
    PROTOTYPE_SCOPE.md             # Step 0/구현이 갱신, 패키지에는 없음
    HANDOFF.md                      # Step 0/구현이 갱신, 패키지에는 없음
    element-lab-v01/
      01_ART_DIRECTION.md
      02_PROTOTYPE_SPEC.md
      03_CODEX_IMPLEMENT.md
      04_QA_CHECKLIST.md
  references/element-lab-v01/
    README.md
    reference-manifest.json
    Hwanjo_Reference_Character.png
    Hwanjo_Reference_Stage1.png
    Hwanjo_Reference_4Concept.png
    Hwanjo_Reference_VFX1.png
    Hwanjo_Reference_VFX2.png
```

원본 컨셉 이미지들은 Assets에 넣지 않는다. 이번 원본은 제작 지시·참고용이다.
새 게임용 산출물만 Codex가 `Hwanjo_v2_Test/Assets/ElementLab/Art` 등에 만든다.
기존 AGENTS·Skill·PROTOTYPE_SCOPE를 이 ZIP으로 덮어쓰지 않는다. 같은 이름의 패키지를 예전에 수정했다면 먼저 비교한다.

## Step 0과의 관계

이전에 받은 `HWANJO_V2_STEP0_SETUP.md`는 환경 확인과 문서/Skill 준비 단계다.
Step 0 작업이 진행 중이면 먼저 완료하게 한다. 이번 구현 요청을 같은 프로젝트에 동시에 실행하지 않는다.
완료 보고가 아직 없으면 결과를 확인한다. 환경 준비가 막힌 상태에서 이 요청으로 게임 제작을 시작하지 않는다.
이 패키지에는 Step 0 파일을 중복 넣지 않았다.

## 실행 전

Unity Editor가 이 프로젝트를 열고 있으면 저장하고 닫는다. 다른 프로젝트 프로세스는 건드리지 않는다.
PowerShell에서:

```powershell
Set-Location -LiteralPath "C:\Users\JEYJEY\Desktop\Hwanjo_v2"
git branch --show-current
git status --short
```

기대 작업 브랜치는 `prototype/element-lab-v01`이다. 이미 있으면 재생성하지 않는다.
main만 있고 아직 작업 브랜치를 만들지 않았다면, 사용자 변경을 검토하고 이전 Step 0 안내대로 작업 브랜치를 준비한다.
Step 0 문서와 새 패키지가 untracked/modified인 것은 읽고 보존해야 하는 입력이다. `git reset --hard`나 `clean`으로 없애지 않는다.

## Codex 실행

기존 Step 0 세션에서 이어가도 된다. 새 세션이면:

```powershell
codex -C "C:\Users\JEYJEY\Desktop\Hwanjo_v2" --sandbox workspace-write --ask-for-approval on-request
```

이것은 대화형 구현이다. 추가 Unity 실행 권한이 필요하면 실제 명령을 확인해 승인한다.
무승인·전체 PC 접근 모드나 이전 overnight 런처를 가져오지 않는다.
현재 CLI가 위 옵션을 지원하지 않으면 `codex --help`로 확인하고, 오류 내용을 근거로 조정한다.

Codex 입력창에 아래를 붙여 넣는다.

```text
Step 0이 완료되었다면 다음 구현 단계로 진행해줘.

docs/element-lab-v01/03_CODEX_IMPLEMENT.md를 작업 요청으로 읽고,
01_ART_DIRECTION.md, 02_PROTOTYPE_SPEC.md, 04_QA_CHECKLIST.md를 따라
Hwanjo v2 Element Lab v0.1을 실제 구현·검증·Windows 빌드까지 진행해줘.

references/element-lab-v01의 원본 이미지 5장을 실제로 열어서 봐줘.
캐릭터는 Character, 숲/화면 비율은 Stage1, 세계 분위기는 4Concept,
베기 궤적과 타격 강약은 VFX1/VFX2를 참고해.
쌍검·대도·3타 콤보·폼체인지는 옛 이미지의 내용이므로 구현하지 마.
현재는 동일 주인공, 검 하나, 단타 1회와 차지/검흔 시스템이야.

기존 AGENTS와 hwanjo-v2-prototype Skill이 있으면 사용하고,
사용자 결정과 임시 구현값을 구분해서 문서에 기록해줘.
Step 0이 미완료/차단 상태라면 구현하지 말고 필요한 조치만 보고해.
Git commit/push/브랜치 변경은 하지 마.
```

이미지 열기 기능이 없으면 이름만 보고 추측하게 하지 않는다.
지원되는 CLI에서는 `--image`로 원본을 첨부할 수 있다. 아래는 새 세션에 다섯 장을 명시적으로 전달하는 대안이다.
위 세션을 실행 중인 채로 두 번째 세션을 중복 시작하지 않는다.

```powershell
Set-Location -LiteralPath "C:\Users\JEYJEY\Desktop\Hwanjo_v2"
$refs = @(
    "references/element-lab-v01/Hwanjo_Reference_Character.png",
    "references/element-lab-v01/Hwanjo_Reference_Stage1.png",
    "references/element-lab-v01/Hwanjo_Reference_4Concept.png",
    "references/element-lab-v01/Hwanjo_Reference_VFX1.png",
    "references/element-lab-v01/Hwanjo_Reference_VFX2.png"
)
$missing = @($refs | Where-Object { -not (Test-Path -LiteralPath $_) })
if ($missing.Count -gt 0) { throw ("레퍼런스 누락: " + ($missing -join ", ")) }
$imageList = $refs -join ","
codex -C "C:\Users\JEYJEY\Desktop\Hwanjo_v2" --sandbox workspace-write --ask-for-approval on-request --image "$imageList"
```

## 결과를 받을 위치

- 씬: `Hwanjo_v2_Test/Assets/ElementLab/Scenes/ElementLab.unity`
- 빌드 목표: `Builds/ElementLabV01/HwanjoElementLab.exe` + 해당 출력 폴더 전체
- 튜닝/실제 결정: `docs/PROTOTYPE_SCOPE.md`
- 검증: `docs/PROTOTYPE_VALIDATION.md`
- 아트/생성 기록: `docs/ART_ASSET_MANIFEST.md`
- 인계: `docs/HANDOFF.md`

이는 **생성될 목표 경로**다. 패키지를 푼 것만으로 빌드나 Unity 씬이 생성되지는 않는다.

## 이번 패키지의 검수 범위

원본 PNG 복사 일치, 명세의 최신 확정값, 옛 레퍼런스 규칙 제외, 문서/상대 링크/파일 경로와 ZIP 구조를 확인했다.
사용자 Windows/Unity 프로젝트에서 실행·빌드·아트 품질을 검증한 것은 아니다.
일반 적 직접 동결, 검흔 2초, release 판별, 소화→Wet 한 전이 등은 시작 가안으로 명시했다.
새 사용자 결정이 있으면 이 가안보다 우선한다.

## 공식 사용 안내

- CLI 옵션/권한: https://developers.openai.com/codex/cli/reference
- 이미지 첨부: https://learn.chatgpt.com/docs/image-inputs
- 이미지 생성: https://learn.chatgpt.com/docs/image-generation
- 저장소 Skill: https://developers.openai.com/codex/skills
- Windows sandbox: https://developers.openai.com/codex/windows
