# Step 0 환경 확인 결과

확인일: 2026-09-24, Asia/Seoul. 실행 식별자: `20260924T225433+0900`.

**Step 0 준비 상태: READY.** 승인된 Unity 실행에서 기존 프로젝트의 패키지 해석·Import·스크립트 컴파일·정상 종료를 확인했다. 이는 환경과 기준 문서의 준비 완료를 뜻한다. 게임·아트 제작, 플레이·UI·시각 검증, 테스트 통과, Windows 빌드·성능 검증을 뜻하지 않는다.

## 저장소와 변경 전 상태

| 항목 | 실제 확인 결과 |
| --- | --- |
| Git 루트 | `C:\Users\JEYJEY\Desktop\Hwanjo_v2` |
| Unity 프로젝트 | `C:\Users\JEYJEY\Desktop\Hwanjo_v2\Hwanjo_v2_Test` |
| 브랜치 | `prototype/element-lab-v01` — 요청 브랜치와 일치 |
| HEAD | `7541ea07655be761d5bb399da17b85b8029d8e8d` |
| origin fetch/push | `https://github.com/edan2251/Hwanjo_v2.git` — 로컬 Git 설정 확인, 원격 접속 검증은 하지 않음 |
| staged / unstaged | 시작 시 모두 없음 |
| untracked | 시작 시 `HWANJO_V2_STEP0_SETUP.md`만 존재 |
| 기존 작업 지침 | 저장소 내 기존 `AGENTS.md`·Skill 없음. 확인한 상위 디렉터리에도 `AGENTS.md` 없음 |
| 요청서 SHA-256 | `E88CD4575482B0980D42FC7D59341B796EB7BD624370D8C0A7CF77E57F3EA49A` — 최종 비교에서 동일 |

`git rev-parse --show-toplevel`, `git branch --show-current`, `git rev-parse HEAD`, `git remote -v`, `git diff --cached --name-status`, `git diff --name-status`, `git status --short --untracked-files=all`로 조사했다. 변경 전 추적 파일 50개의 SHA-256과 상태는 실행 폴더의 `baseline.json`, 파일 목록은 `tracked-before.stdout.txt`에 보존했다. Git 기록·브랜치·원격을 변경하지 않았다.

## 파일 추적과 ignore

- 추적 파일 50개 중 `Assets` 19개(그중 `.meta` 11개), `Packages` 2개, `ProjectSettings` 25개다. 나머지는 루트 README·ignore, 프로젝트 내부 ignore·`.vsconfig`다.
- Assets의 파일·하위 폴더에 필요한 `.meta` 누락 0개, 미추적 `.meta` 0개다. 캐시·빌드·로그의 추적 파일은 0개다.
- 루트와 `Hwanjo_v2_Test/.gitignore`를 각각 읽었다. 내부 ignore가 중첩 프로젝트의 `Library`, `Temp`, `Obj`, `Build`, `Builds`, `Logs`, `UserSettings`를 제외함을 `git check-ignore -v --no-index`로 확인했다. 기존 내부 ignore는 수정하지 않았다.
- 루트 `Builds/`는 기존 `/[Bb]uilds/`에 이미 해당한다. 누락된 `/artifacts/`만 루트 `.gitignore`에 추가했다. 기존 CRLF를 유지했다.
- 필요한 Assets·Packages·ProjectSettings의 추적 파일 46개는 `git check-ignore --no-index -v --stdin`에서 매칭 없음(종료 코드 1, 정상적인 비매칭)이다. 캐시·빌드·진단 경로 예시는 모두 매칭됨(종료 코드 0)을 확인했다.
- 시작부터 존재한 `Library`, `Logs`, `UserSettings`, `.sln`은 캐시/로컬 설정으로 유지했다. 캐시 삭제·전체 재임포트 강제·복사본 프로젝트 제작을 하지 않았다.

근거: `static-audit.json`, `ignore-probes.stdout.txt`, `required-paths-ignore.stdout.txt` 및 각각의 stderr 파일. probe 경로는 ignore 판정을 위한 문자열이며 실제 파일을 만들지 않았다.

## Unity 버전과 패키지

- `ProjectSettings/ProjectVersion.txt`: `6000.3.23f1 (09d2ecc7fb28)`.
- 실제 Editor: `C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe`.
- 실행 파일 FileVersion: `6000.3.23.643820`. 실제 승인 실행에서도 해당 Editor를 사용했다.
- Windows 64비트, OS 버전 `10.0.26200.0`, 초기 조사 PowerShell `5.1.26100.9444`.

| 패키지 | manifest / lock / 로컬 캐시 버전 | lock source |
| --- | --- | --- |
| URP (`com.unity.render-pipelines.universal`) | 모두 `17.3.0` | builtin |
| Input System (`com.unity.inputsystem`) | 모두 `1.20.0` | registry |
| 2D Animation (`com.unity.2d.animation`) | 모두 `13.0.6` | registry |
| Test Framework (`com.unity.test-framework`) | 모두 `1.6.0` | builtin |

manifest의 모든 직접 의존성과 lock 버전에 불일치가 없다. 승인된 실행에서 Package Manager가 65개 패키지를 등록했다. 패키지 삭제·업데이트·버전 변경은 하지 않았다.

## 렌더러·입력·초기 씬

**2D Renderer 연결은 파일 참조를 따라 확인했다. 시각 출력은 미검증이다.**

1. `ProjectSettings/GraphicsSettings.asset`의 `m_CustomRenderPipeline`은 `{fileID: 0}`이다.
2. `ProjectSettings/QualitySettings.asset`의 6개 품질 단계 모두 `customRenderPipeline`에서 GUID `681886c5eb7344803b6206f758bf0b1c`를 참조한다. 현재 품질과 Standalone 기본 품질은 인덱스 5(Ultra)다.
3. 위 GUID는 `Assets/Settings/UniversalRP.asset.meta`와 일치한다.
4. `UniversalRP.asset`의 `m_RendererDataList[0]`는 GUID `424799608f7334c24bf367e4bbfa7f9a`, `m_DefaultRendererIndex`는 0이다. GUID는 `Renderer2D.asset.meta`와 일치한다.
5. `Renderer2D.asset`의 스크립트 GUID `11145981673336645838492a2d98e247`는 로컬 URP 패키지의 `Runtime/2D/Renderer2DData.cs.meta`와 일치한다. URP asset의 스크립트 GUID도 패키지의 `UniversalRenderPipelineAsset.cs.meta`와 일치한다.

따라서 Graphics 기본 슬롯이 비어 있다는 사실만으로 미연결로 판정하지 않는다. 품질별 override와 실제 Renderer2D 참조가 있다. Batch Mode는 `-nographics`로 실행했으므로 게임 화면과 렌더링 품질을 검증하지 않았다.

- 입력: `ProjectSettings.asset`의 `activeInputHandler: 1`. 설치된 Input System 소스 `InputSystem/Editor/Settings/EditorPlayerSettingHelpers.cs`의 enum에서 `NewInputSystem = 1`임을 확인했다. 실제 로그에도 Input System 초기화와 종료가 기록됐다.
- `EditorBuildSettings.asset`의 프로젝트 입력 액션 GUID `2bcd2660ca9b64942af0de543d8d7100`는 `Assets/Settings/InputSystem_Actions.inputactions.meta`와 일치한다. 기존 액션 맵은 Player(9개 액션, 35개 바인딩)와 UI(10개 액션, 38개 바인딩)다. 새 기획의 키 입력을 구현하거나 시험하지 않았다.
- 빌드 목록에는 활성화된 `Assets/Scenes/SampleScene.unity` 하나가 있다. GUID는 `.meta`와 일치한다. 씬에는 직교 Main Camera와 Global Light 2D가 있으며 카메라 Renderer index는 기본값을 쓰는 `-1`이다. `Assets/Settings/Scenes/URP2DSceneTemplate.unity`는 별도 템플릿 씬이다.
- 프로젝트의 `Assets`·`Packages`에 사용자 C#·asmdef·asmref가 없다. **프로젝트 테스트 0개 / 미작성**이며 테스트 실행은 하지 않았다. Test Framework 패키지나 패키지 내부 테스트의 존재를 게임 테스트로 세지 않았다.

## Windows 빌드 준비 상태

설치 구성은 확인했으며 실제 Windows Player 빌드는 요청 범위 밖이므로 **미실행**이다.

- Editor의 `Editor/Data/PlaybackEngines/windowsstandalonesupport`와 Windows 확장 DLL, win64 Mono·IL2CPP 플레이어 variations가 존재한다.
- 해당 Editor의 `modules.json`에 `windows-il2cpp` 선택 상태가 true이며 실제 IL2CPP variations도 확인했다. 설치 메타데이터만으로 빌드 성공을 주장하지 않는다.
- `vswhere.exe -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`는 Visual Studio 2022 Community와 2026 Community 설치를 반환했다.
- MSVC 디렉터리 `14.44.35207`, `14.51.36231`, Windows SDK `10.0.26100.0`의 `Windows.h`·x64 `kernel32.lib` 존재를 확인했다.
- 프로젝트 `.vsconfig`에는 `Microsoft.VisualStudio.Workload.ManagedGame`이 있다. PlayerSettings의 `scriptingBackend`에는 Android만 명시돼 있으며 Standalone의 최종 백엔드를 이번에 설정·빌드하지 않았다.
- 로그의 `Native extension for WindowsStandalone target not found` / `WebGL target not found` 메시지는 보존했다. Editor Import/컴파일은 완료됐으나 이 메시지를 근거 없이 Windows 빌드 성공 또는 실패로 판정하지 않는다.

## 실제 Unity 실행과 권한

초기 `Get-CimInstance Win32_Process`는 접근 거부(HRESULT `0x80041003`)로 실패했다. 대체 읽기 전용 `Get-Process -Name Unity`에서 실행 중인 Editor가 없고 프로젝트 잠금 파일도 없음을 확인한 뒤 실행했다. 다른 Unity/Hub/라이선싱 프로세스를 종료하거나 잠금 파일을 지우지 않았다.

실행 폴더(아래 파일명은 이 폴더 기준):

`C:\Users\JEYJEY\Desktop\Hwanjo_v2\artifacts\setup\20260924T225433+0900\`

| 시도 | 실제 결과 | 종료 코드 / 기록 |
| --- | --- | --- |
| 최초 PowerShell 실행 래퍼 | `Start-Process`가 환경 키 `Path`/`PATH` 중복 예외로 Unity를 시작하지 못함 | Unity 종료 코드 없음(null). `unity-sandbox.result.json`; stdout/stderr는 빈 파일, Editor 로그 없음 |
| 샌드박스 내부 Unity 실행 | .NET Process로 실행됨. 사용자 Unity 폴더·UPM 캐시 접근 거부, 라이선스 IPC 거부/시간 초과 후 패키지 해석 실패 | **1 / BLOCKED(권한)**. `unity-batch.result.json`, `unity-batch.editor.log`, `unity-batch.stdout.log`, `unity-batch.stderr.log` |
| 해당 명령만 승인 후 실행 | 23:00:35–23:01:19 KST. 패키지 해석·Import·스크립트 컴파일·종료 완료 | **0 / PASS**. `unity-approved.result.json`, `unity-approved.editor.log`, `unity-approved.stdout.log`, `unity-approved.stderr.log` |

첫 래퍼 실패는 Unity의 컴파일 실패가 아니다. .NET Process로 출력 스트림을 분리하고 숨김 실행했으며 전역 환경변수나 권한 설정을 바꾸지 않았다. 이후 권한 차단이 확인되어 요청서 3절 10항에 따라 아래 진단의 샌드박스 밖 실행만 승인 요청해 허용받았다. 같은 샌드박스 오류로 반복 실행하지 않았다. Unity 자체의 라이선스 재연결 기록과 에이전트의 재실행을 구분한다.

승인된 실제 실행 파일과 인자:

```text
"C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\JEYJEY\Desktop\Hwanjo_v2\Hwanjo_v2_Test" -logFile "C:\Users\JEYJEY\Desktop\Hwanjo_v2\artifacts\setup\20260924T225433+0900\unity-approved.editor.log"
```

승인 실행의 stdout/stderr는 각각 0바이트다. 전체 Editor 로그는 별도 파일에 있다. 컴파일 오류 무시 옵션, 버전·패키지 변경, 테스트 게임 빌드 제작, 강제 전체 재임포트 옵션은 사용하지 않았다.

완료 근거(`unity-approved.editor.log`):

- 패키지 해석 완료(174행), 패키지 65개 등록(176행).
- `Tundra build success`(2394행), script compilation 16.390294초(2395행), 어셈블리 reload 완료.
- `Asset Pipeline Refresh` 31.787초(2476행), Import 9개, 원본 Asset 파일 추가·변경·이동·삭제 모두 0개(2483행).
- `Exiting batchmode successfully now!` 및 `return code 0`(2550–2551행), 프로세스 종료 코드도 0.

**남아 있는 진단 메시지와 한계:** 시작 시 기존 라이선스 채널의 protocol `1.18.3` handshake 오류(505), 클라이언트 signature 검증 Code 10/validation 메시지가 있다. 같은 실행에서 버전별 클라이언트 연결·라이선스 초기화가 완료됐다. 종료 과정에는 `Curl error 42: Callback aborted`, `abort_threads`, `debugger-agent: Unable to listen on 3244`가 남아 있다. 이들의 근본 원인을 수정하거나 디버거 연결을 검증하지 않았다. 해당 실행의 Import/컴파일 및 정상 종료를 막지는 않았으며, 로그에 C# 컴파일 실패는 확인되지 않았다. 경고·오류 문자열을 삭제해 성공으로 처리하지 않았다.

전체 로그의 순서와 오류는 보존하되 세션·상관·머신·라이선스 식별 필드 5개의 값을 `[REDACTED]`로 가렸다. 비식별화 내역과 최종 해시는 `log-redaction.json`에 있다. 가리기 전 별도 복사본은 남기지 않았다.

## 문서·Skill 및 이미지 도구 검증 수준

- `AGENTS.md`, 저장소 전용 Skill 하나, 기획 기준·환경 보고서·인계 문서를 준비하고 README에 경로 안내를 추가했다.
- `skill-creator`의 `quick_validate.py`를 실제 실행했으나 현재 Python에 PyYAML(`yaml`)이 없어 종료 코드 1이다. 이 자동 검증 항목은 **BLOCKED**이며 성공으로 기록하지 않는다. 프로그램/패키지를 설치하지 않았다. 기록: `skill-validator.result.json`, stdout/stderr 파일.
- 별도로 이번 Skill의 두 단순 문자열 frontmatter(name/description), 이름·폴더 일치, 길이·허용 문자·미완성 placeholder 없음·단일 파일 구성을 정적 점검했고 PASS다(`skill-static-validation.json`). 내용은 허용 범위와 단일 기획 기준을 유지하는지 직접 검토했다. 문서 링크·UTF-8·변경 범위 최종 점검은 `final-validation.json`에 기록한다. 자동 Skill 선택이나 실제 구현 행동을 실행해 검증한 것은 아니다.
- 이미지 입력용 `view_image`와 생성·편집용 `image_gen.imagegen`은 이번 세션의 **도구 명세상 노출만 확인**했다. 실제 이미지 읽기·생성·편집·외부 유료 API 호출은 하지 않았다. 도구의 실동작 성공으로 기록하지 않는다.
- 이번 대화에 캐릭터/화풍 레퍼런스가 제공되지 않았고 프로젝트 Assets에서도 이미지 아트 파일을 발견하지 못했다. 폴더 스크린샷을 캐릭터 레퍼런스로 취급하지 않는다. 아트 준비 미완료는 Step 0 문서·환경 준비의 차단 사유가 아니다.

## 보존 및 다음 단계의 경계

추적 파일 바이트 비교에서 변경은 루트 `.gitignore`, `README.md`뿐이다. Unity Import 후 `Assets`·`Packages`·`ProjectSettings` 추적 파일 변경과 추가 원본 파일은 없다. 사용자 요청서의 해시도 동일하다. 기존 v1을 조사·복사·수정하지 않았으며 게임 코드·씬·아트·야간 실행기를 만들지 않았다. Git add/commit/push 등은 실행하지 않았다.

환경 진단은 완료됐으나 다음 구현은 새 사용자 요청이 있어야 시작한다. 구현별 미정사항은 [PROTOTYPE_SCOPE.md](PROTOTYPE_SCOPE.md)에 유지하고 다음 시작점은 [HANDOFF.md](HANDOFF.md)에 기록한다. 향후 샌드박스 안에서 Unity를 실행하면 같은 권한 차단이 재발할 수 있으며, 이번 승인으로 전역 실행 권한이 바뀐 것은 아니다.
