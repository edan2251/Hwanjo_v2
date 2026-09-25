# Hwanjo v2 — Element Lab v0.1 인계

2026-09-25 KST. **PASS — 구현·검증·Windows 빌드 완료 후 여기서 멈춘다.** 사용자/팀 아트 승인과 사람의 플레이 평가는 별도다. 브랜치 `prototype/element-lab-v01`, HEAD `7541ea07655be761d5bb399da17b85b8029d8e8d` 유지. Git add/commit/push/브랜치 변경 없음.

## 실행과 구현

- Windows: `Builds/ElementLabV01/HwanjoElementLab.exe`와 같은 폴더의 필수 데이터 전체.
- Unity 6000.3.23f1: `Hwanjo_v2_Test/Assets/ElementLab/Scenes/ElementLab.unity`.
- A/D 이동, Space 점프, Shift 대시, J release 단타/0.5초 차지, 1~4 불/물/바람/얼음, R 전체 초기화, Esc 메뉴, F1 로그, F2 더미, F3 타격감, F9 PNG. 아래 버튼으로 구역 이동.
- 검1개/단타1회/차지1.75R/검흔1슬롯과 Trait 기반 EL00~EL10 반응, 더미 런타임 편집, 환경6종, 기본 적1종, 다리·수면 응용 구간을 새로 구현했다. v1 코드·폼/콤보·야간 실행기를 가져오지 않았다.
- 원본5장 모두 열어 역할별로 참고했고 새 주인공10종 동작·숲 배경과 VFX를 연결했다. 생성 도구4회(허용8회 이내). 자세한 리소스·가공·fallback 여부는 [ART_ASSET_MANIFEST.md](ART_ASSET_MANIFEST.md).
- 확정 사용자 결정과 실제 채택한 임시값은 [PROTOTYPE_SCOPE.md](PROTOTYPE_SCOPE.md)에서 구분한다. 검흔2초/R1.6/피해10 등은 조절 가능한 시작값이며 최종 게임 합의가 아니다.

## 마지막 검증 지점

- EditMode33/33, PlayMode23/23, 실패0/skip0. XML과 전체 로그를 읽었다.
- 최종 `22-delivery-build`: exit0, errors0/warnings0. `23-delivery-replay`: 실제 Windows 그래픽 + 자동 입력11검사 PASS, PNG37개, 정상 종료0.
- 실제 Windows UI/키 조작: 프리셋·특성·상태·동결조건·Apply 거부/적용·두 가지 Reset, J/2/Space/F1/F2/F3/F9/R/Esc, 편집 중 게임 키 차단, 적 피격과 초기화 확인. 직접 긴 키 유지와 자동 입력 재생의 검증 범위는 구분했다.
- `24-delivery-ui`: 최종 빌드의 UI 재검증과 X 버튼 정상 종료0, Native Crash 없음. 검증용 게임/Editor를 남기지 않았다.
- 실제 게임 PNG와 동작 확대 표본을 열어 확인했다. 모든 기록: `artifacts/element-lab/20260924T232704+0900/`. 상세 설명과 재현 명령: [PROTOTYPE_VALIDATION.md](PROTOTYPE_VALIDATION.md).

## 남은 문제와 다음 사람이 확인할 것

- 종료 시 `GarbageCollector disposing of ComputeBuffer` 경고가 남는다. 발생 주체와 장기 영향은 미확정. 초기 Mono Native Crash 종료 기록은 보존했고, 종료 경로 수정 후 메뉴/X/자동 검증 종료에서 재현되지 않았다.
- 실측 FPS·장시간 안정성·다른 PC/해상도와 최종 미술 승인은 미검증. 연속 PNG 저장은 성능 측정으로 사용하지 않는다.
- 사용자 원본·AGENTS·Skill·패키지는 보존했다. Unity 자동 저장으로 Settings3개/ProjectSettings2개와 Resources·SceneTemplate 파일이 변했다. 목록과 전체 diff를 검증 문서/RUN에 남겼으며 임의 복구하지 않았다.
- Step 0의 선택적 Skill validator는 PyYAML 미설치 상태 그대로이며 새 도구/패키지를 설치하지 않았다. 게임 환경과 빌드 검증의 성공을 이 검사 성공으로 확대하지 않는다.

다음 작업은 사용자의 새 요청 범위에서 시작한다. 사람이 플레이하며 답할 질문은 다음3개다.

1. 서로 다른 덩굴·밧줄에서도 같은 특성 반응을 예상할 수 있는가?
2. 차지→속성 변경→검흔 재타격을2초 안에 하는 흐름이 급하거나 답답한가?
3. 적의 상태와 바람/얼음 반응이 퍼즐 규칙 이해에 도움이 되는가?

<details>
<summary>이전 Step 0 인계 원문 — 당시 상태 기록</summary>

아래 READY·미검증·중단 표현은 2026-09-24 Step 0 시점의 기록이다. 현재 상태는 위 Element Lab 인계를 따른다. 원문 별도 사본은 RUN의 `HANDOFF.step0.md`에도 보존했다.

# Hwanjo v2 — Step 0 인계

2026-09-24 / 브랜치 `prototype/element-lab-v01` / HEAD `7541ea07655be761d5bb399da17b85b8029d8e8d`.

**READY — 환경과 기준 문서 준비 완료. 이 단계에서 멈춘다.** 게임 구현·아트 제작·테스트 통과·Windows 빌드 완료를 뜻하지 않는다.

## 이번에 완료한 일

- 저장소 루트와 `Hwanjo_v2_Test/` Unity 프로젝트를 구분하고 Git 상태·추적 파일·`.meta`·내부/루트 ignore를 확인했다.
- `AGENTS.md`에 공통 지침, `.agents/skills/hwanjo-v2-prototype/SKILL.md`에 저장소 전용 절차를 작성했다. Skill은 하나의 지침 파일이며 런처나 보조 스크립트 묶음이 없다.
- [PROTOTYPE_SCOPE.md](PROTOTYPE_SCOPE.md)에 사용자 확정·추가 요청·추천·미정을 분리했다. 전체 기획과 숫자는 이 문서만 기준으로 삼는다.
- [ENVIRONMENT_AUDIT.md](ENVIRONMENT_AUDIT.md)에 로컬 Editor·패키지·Renderer2D 참조·입력·씬·Windows 빌드 설치 구성과 실제 진단 결과를 기록했다.
- 루트 `.gitignore`에 누락된 `/artifacts/`만 추가했다. `Builds/`는 기존 규칙이 적용되며 Unity 내부 ignore도 기존 것을 사용한다. README에는 프로젝트와 문서 경로만 추가했다.
- 기존 Unity 프로젝트를 실제 Batch Mode로 열었다. 최초 래퍼 예외는 Unity 미실행, 샌드박스 실행은 권한 문제로 종료 코드 1, 해당 명령 승인 후 실행은 Import·컴파일·정상 종료와 코드 0을 확인했다.

진단 파일: `artifacts/setup/20260924T225433+0900/`. 승인 실행의 `unity-approved.result.json`과 `unity-approved.editor.log`, stdout/stderr를 함께 확인한다. 실패 기록도 남겼으며 비밀·식별 필드만 가렸다.

## 보존과 변경 요약

- 시작 시 사용자 변경은 untracked 요청서 `HWANJO_V2_STEP0_SETUP.md`뿐이었다. 수정하지 않았고 SHA-256 동일성을 확인했다.
- 추적 파일 변경: `.gitignore` 3줄 추가, `README.md` 9줄 추가/1줄 삭제. README의 기존 제목은 유지했다.
- 새 작업 파일 5개: `AGENTS.md`, 전용 `SKILL.md`, `docs/PROTOTYPE_SCOPE.md`, `docs/ENVIRONMENT_AUDIT.md`, 이 인계 문서.
- Unity의 `Assets`·`Packages`·`ProjectSettings`에는 의도적 변경도 Import에 의한 추적 파일 변경도 없다. 기존 캐시를 삭제하지 않았다.
- 게임 코드·씬·아트는 제작하지 않았다. 기존 환조 v1을 찾거나 가져오지 않았으며 야간 실행기도 만들지 않았다.
- Git add/commit/push/merge/switch/reset/restore/clean/stash를 하지 않았다. HEAD와 브랜치는 그대로다.

작업 후 `git status --short --untracked-files=all`:

```text
 M .gitignore
 M README.md
?? .agents/skills/hwanjo-v2-prototype/SKILL.md
?? AGENTS.md
?? HWANJO_V2_STEP0_SETUP.md
?? docs/ENVIRONMENT_AUDIT.md
?? docs/HANDOFF.md
?? docs/PROTOTYPE_SCOPE.md
```

`artifacts/`와 Unity 캐시는 ignore 대상이다. `git diff`는 untracked 신규 문서 본문을 보여주지 않으므로 새 파일은 별도로 읽어 검토한다. 최종 상태·diff·stdout/stderr와 보존 검증은 실행 폴더의 `final-*` 파일에 남긴다. README의 LF→CRLF Git 경고는 stderr의 줄바꿈 경고이며 삭제 파일 목록이 아니다.

## 남은 문제와 미검증

- 승인 실행에서도 라이선스 handshake·signature 관련 메시지와 종료 시 curl/Mono thread/debugger 메시지가 남았다. 같은 실행에서 라이선스 초기화와 Import/컴파일은 완료됐다. 근본 원인 해소·디버거 연결은 검증하지 않았다.
- 향후 Unity 실행은 샌드박스 사용자 캐시/라이선스 접근 제한으로 다시 막힐 수 있다. 이번에는 해당 진단 명령만 승인받았으며 전역 설정을 바꾸지 않았다.
- Windows Mono·IL2CPP 모듈, MSVC, SDK의 존재를 확인했지만 실제 빌드는 미실행이다. 로그의 native extension 메시지도 보존했다.
- 프로젝트 테스트는 **0개 / 미작성**이다. 플레이·게임 입력·반응·UI·시각 출력·빌드·성능은 미검증이다.
- Skill의 구조와 내용은 정적 점검했지만 기본 `quick_validate.py`는 PyYAML 누락으로 **BLOCKED**다. 자동 검증 성공이나 실제 자동 선택 확인으로 기록하지 않았다. 필요한 경우 이후 허용된 Python 환경에서 이 검사만 실행한다. 이번에는 설치하지 않는다.
- 이미지 읽기/생성은 도구 명세 노출만 확인했으며 실동작은 미검증이다. 캐릭터/화풍 레퍼런스와 실제 사용 권리 확인이 남아 있다.

위 한계는 READY의 의미를 환경·기준 문서 준비로 제한한다. 이미 완료된 Step 0 진단을 전투나 게임 품질 검증으로 확대하지 않는다.

## 다음 작업 시작점 — 이번에는 실행하지 않음

1. 다음 사용자 요청의 허용 범위를 확인한 뒤 `AGENTS.md`, 이 문서, 환경 보고서, 전용 Skill을 읽는다. 브랜치·사용자 변경과 프로젝트가 Editor에서 열려 있는지 다시 확인한다.
2. 기획은 `PROTOTYPE_SCOPE.md`만 참조한다. 그 문서의 미정 표에서 이번 구현을 좌우하는 항목만 결정한다. 이미 확정된 키·차지 기준·거리 비율·검흔 개수·속성 선택·한 단계 반응 원칙은 다시 묻지 않는다.
3. 미정 항목은 검흔 수명과 규격/교체, 탭·홀드 발동 시점, 차지 중 조작, 실제 R, 반응 묶음·상태 공존/복귀, 적의 동결·바람 반응 조건, 바람 검흔·얼음 구조 제약, 캐릭터 레퍼런스다. 구체 내용과 확정/추천의 경계는 기획 기준의 표를 따른다.
4. 구현이 명시적으로 요청되면 지정 범위만 작업하고 필요한 검증 결과를 따로 남긴다. Unity 실행이 권한에서 차단될 때만 필요한 해당 명령의 승인을 요청하고, 실패·미실행은 그대로 기록한다.

현재 인계 시점에는 다음 구현을 시작하지 않는다.

</details>
