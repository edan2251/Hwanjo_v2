# Element Lab v0.1 검증 기록

검증일: 2026-09-24~25 KST. **PASS — 요청한 기능·리소스 구현, 자동 검사, Windows 빌드와 실제 UI/입력/화면·정상 종료 확인 완료.** 종료 시 ComputeBuffer 정리 경고는 남아 있다. 사용자/팀 아트 승인과 사람의 재미 평가는 별개다.

## 범위와 환경

Step 0의 READY 근거와 승인 Import 성공을 확인한 후 후속 구현 요청을 수행했다. Step 0의 선택적 Skill validator만 PyYAML 누락으로 차단되어 있었으며 게임 환경 차단은 아니었다. 당시 보고서는 `ENVIRONMENT_AUDIT.md`에 보존했다.

Unity `6000.3.23f1 (09d2ecc7fb28)`, 프로젝트 `Hwanjo_v2_Test`, 브랜치 `prototype/element-lab-v01`, HEAD `7541ea07655be761d5bb399da17b85b8029d8e8d`. 기존 Input System/URP/패키지 버전을 사용했다. 사용자 안내 경로 대신 실제 `docs/01~04*.md`와 `element-lab-v01/` 원본을 읽었다. 원본5장 모두 실제 열었으며 게임 리소스로 원본 시트 자체를 잘라 쓰지 않았다.

로그 기준 폴더는 `artifacts/element-lab/20260924T232704+0900/`(이하 RUN)이다. `*.result.json`은 실제 명령·PID·시각·종료 코드를 기록한다. `*.editor.log`는 Unity 전체 로그이며 stdout/stderr는 별도 파일이다. 라이선스/세션/기기 식별 필드만 가린 사실을 기록했다. 실패 로그도 보존했다.

## 자동 검사

| 실행 | 실제 결과 | 근거 |
| --- | --- | --- |
| 최초 공통 반응 EditMode | 30/30 성공, 실패0, skip0, exit0 | 01-core-editmode.xml / result / editor.log |
| 초기 런타임 PlayMode | 18개 중17 성공, 입력1 실패, exit2 | 03-runtime-playmode.xml |
| InputTestFixture 적용 직후 | 18개 중4 성공/14 실패, skip0, exit2. 중복 장치 제거로 정리가 중단되어 다음 검사에 영향. 실패 보존 | 04-input-playmode.xml |
| 입력 fixture 정리 수정 | 18/18 성공, 실패0, skip0, exit0 | 05-input-playmode.xml |
| 아트 포함 EditMode | 33/33 성공, 실패0, skip0, exit0 | 07-art-editmode.xml |
| 입력 취소·적 물리·타격감 | 21/21 성공, 실패0, skip0, exit0 | 08-final-playmode.xml |
| 창 키 이벤트 호환/중복 방지 | 22/22 성공, 실패0, skip0, exit0 | 15-keyboard-playmode.xml |
| R/순간이동 지면 즉시 갱신 | 23/23 성공, 실패0, skip0, exit0 | 20-ground-reset-playmode.xml |

합격한 테스트를 더해 중복 집계하지 않는다. 최종 기능 회귀 집합은 EditMode33 + PlayMode23 =56개다. `.xml`의 total/passed/failed/skipped를 읽었고, 개수만으로 UI/렌더링 통과를 주장하지 않는다. 요청 실패 기대값을 바꾸거나 테스트를 삭제/skip하지 않았다.

재현 명령의 공통 형태(정확한 인자는 각 result JSON 참조):

```text
<검증된 Unity.exe> -projectPath <repo>/Hwanjo_v2_Test -logFile <RUN>/<label>.editor.log
  -batchmode -nographics -runTests -testPlatform EditMode|PlayMode -testResults <RUN>/<label>.xml
<검증된 Unity.exe> -projectPath <project> -logFile <RUN>/<label>.editor.log
  -batchmode -quit -executeMethod Hwanjo.ElementLab.Editor.LabBuild.BuildWindows
```

`tools/element-lab/invoke_unity.py`는 위 단일 명령의 프로세스 종료와 로그만 기록한다. XML 성공을 대신 판정하지 않으며 예약/야간/병렬 실행 기능이 없다. Unity Editor는 한 번에 하나만 실행했다. 샌드박스의 사용자 캐시·라이선스 IPC 제한은 Step 0에서 확인했으므로 필요한 Unity 명령마다 승인을 요청했다. 버전·패키지·전역 권한을 바꾸지 않았다.

## Windows 빌드와 실제 조작

씬은 `Assets/ElementLab/Scenes/ElementLab.unity` 하나를 BuildPipeline에 직접 지정했다. 기존 SampleScene/EditorBuildSettings 목록을 구현 목적으로 변경하지 않았다. 출력은 `Builds/ElementLabV01/HwanjoElementLab.exe`와 같은 폴더의 Data·MonoBleedingEdge·DLL 전체다. 이 폴더를 함께 전달해야 한다.

- 최초 빌드 `09-windows-build`: 성공, exit0, BuildReport errors0/warnings0. 이후 실제 화면/UI 검수 수정마다 다시 빌드했다.
- 최초 샌드박스 Player `10-player`: 실제1280×720 PNG를 생성했으나 데스크톱 도구에서 창을 선택할 수 없었다. 정상 닫기가 종료되지 않아 이 작업의 기록된 PID만 종료했다(exit15). 성공 실행으로 세지 않는다. 최초 Stop-Process 시도 예외도 보존하고 Python 종료 후 실제 종료 코드를 확인했다.
- 승인된 데스크톱 Player `12`, `14`: 실제 마우스 UI를 조작했다. 창 X 종료 후 exit0이었지만 `Native Crash Reporting`이 있어 이 둘을 정상 종료 증거로 세지 않는다.
- `17-player-final`: 실제 창 키 입력으로 2=물, J 단타1회, Space 점프/낙하, F2 패널, 편집 중 J/1/R 차단, F9 캡처, Esc 일시정지와 메뉴 종료를 확인했다. `ELEMENT_LAB_USER_QUIT`, 입력 모듈 Shutdown, exit0, Native Crash 없음. 짧은 D/단독 Shift 입력은 도구의 유지 시간/수정키 전달 한계로 이동 검증으로 세지 않았다.
- UI 실조작 `14`: Preset→Wet Vine, Dry+Wet Apply 거부와 설명, Dry 해제 후 Wet Apply, Reset Target=마지막 Wet 유지, Default Preset=최초 Dry를 화면과 로그에서 확인했다. 자동 테스트와 별개의 실제 Windows 클릭이다.
- 창 키 이벤트는 Unity IMGUI까지 도착하나 Raw Input 상태로 들어오지 않는 경우를 진단했다. `LabKeyboardInterop`는 누락된 상태 전이만 같은 Input System에 전달한다. 물리 입력과 창 메시지 동시 전달 시 중복 공격이 없음을 PlayMode에서 검사했다. 기본 Input System 설정은 그대로다.
- 드롭다운 뒤 토글이 클릭을 가로채던 문제를 실제 클릭으로 발견하여 팝업이 떠 있는 동안 하부 컨트롤을 비활성화했다. 안내문 대비·선택 속성 밑줄·최근 반응 로그 높이도 실제 화면을 보고 수정했다.
- `19-rendered-replay`: 명시적 자동 입력 재생. Run6, 차지, 물→얼음→물 검사를 통과한 뒤 R 직후 지상 상태 초기화 문제를 발견했다. 이 실행은 FAIL이며 최종 성공으로 세지 않는다. 종료 경로 수정 후 X 버튼은 메뉴 종료를 거쳐 exit0, Native Crash 없음을 확인했다.
- 최종 `22-delivery-build`: exit0, BuildReport Succeeded, errors0/warnings0, 107,526,232 bytes. 컴파일 오류 없음. 이 결과의 실행 파일과 데이터가 현재 `Builds/ElementLabV01/`에 있다.
- 최종 `23-delivery-replay`: 실제 그래픽을 켠 Windows 빌드의 자동 입력 재생11개 검사 PASS, exit0. 연속 이동, 유지 중 무피해, 차지1회/1.75R, 제자리 물→얼음 발판→물/원래 수명, 불+바람 전달체1개, 기본 적 직접 동결, 사망을 확인했다. `captures-delivery/`에 실제 PNG37개: Idle4/Run6/Charge2/ChargeRelease6/Slash6/Dash2/Death6 + 주요 상태5개. 오류·Native Crash 없음, ComputeBuffer 종료 경고는 존재.
- 최종 `24-delivery-ui`: F2로 열어 IntrinsicMoisture 선택, Freezable+Meltable 추가, Frozen Apply와 실제 더미 상태 표시를 확인. 구역 버튼으로 적에게 접근해 피격2프레임을 저장하고, R로 HP100·시작 위치·더미Dry 복원, F3 OFF, F1 진단 표시를 직접 키로 확인했다. X 종료는 `ELEMENT_LAB_USER_QUIT` 경로를 거쳤고 exit0, 오류·Native Crash 없음. 이 검증용 Player와 모든 Editor 실행은 종료했다.

실제 캡처는 ScreenCapture로 저장한 PNG다. `captures-final/`은 데스크톱 직접 조작, `captures-replay/`는 화면에 자동 재생 표시가 있는 Input System 이벤트 재생이다. 이미지 생성 출력과 실제 게임 화면을 혼동하지 않는다. 생성 원본/가공 시트는 별도 `art-source/`다.

실제로 열어 본 최종 화면: `captures-final/Slash-HeroRunSlash_09-20260925-003731-542.png`, `captures-delivery/automated-enemy-freeze-20260925-005113-769.png`, `captures-ui/Hit-HeroOtherPoses_10-20260925-005601-115.png` 및 더미 UI 저장본. Run/ChargeRelease의 각6포즈를 실제 게임 PNG에서 발췌한 `Run-actual-game-contact-sheet.png`, `ChargeRelease-actual-game-contact-sheet.png`도 열어 관절 자세·발 기준·검1개·효과 연결을 확대 확인했다. `Slash-actual-game-contact-sheet.png`는 서로 다른 공격에서 처음 확보한 각 포즈를 모은 표본이며 한 공격의 연속 캡처라고 주장하지 않는다. 원시 PNG 시각은 파일명에 보존했다.

## QA 체크리스트 대응

| 항목 | 확인 방법과 범위 |
| --- | --- |
| Q01~04 | EditMode .49/.50 경계, 유지 중 무피해, PlayMode 실제 Input System tap/hold·점프/대시/피격/사망/포커스/패널 취소. Windows J 단타 직접 조작. 긴 키 유지의 Windows 확인은 자동 재생으로 구분. |
| Q05~08 | PlayMode 공통 원점1.6/2.8, 제자리 검흔 재타격, 슬롯 교체/수명/자기 생성 ActionId 차단. 빌드 재생에서 실측 로그와 화면. |
| Q09~17 | EditMode 공통 Trait 규칙, 해동→건조→점화 우선순위, 소화, Cold 조건, 물리 피해 분리, 복수 collider 중복 방지, 바람 경로 하나/고정 제한. |
| Q18~24 | PlayMode 전달 계보/젖은 대상1단계/벽/방향/최초 수명, 지지면·임시 얼음 제한, 물/얼음 collider 복귀, 미지원/동일 속성 반응 없음. |
| Q25~29 | Windows 더미 UI 실조작과 PlayMode Apply/Reset 검사. 편집 중 숫자/공격/리셋 차단. 이 UI에는 숫자 텍스트 입력란이 없으며 enum·토글로 편집한다. |
| Q30 | PlayMode 전체 초기화와 생성·소실·HP·속도·AI·검흔 복원. 추가로 R 직후 지상 판정을 즉시 확인. |
| Q31~37 | EditMode DoT6틱/소화 취소/동결 제한, PlayMode 적 동작과 타격 시점 동일 느린 시계·둔화에서 중력/바람 분리·재부양 제한·동결 공격 취소와 새 예고·사망 초기화. |

## 알려진 경고와 미검증

- Player 종료 시 `GarbageCollector disposing of ComputeBuffer` 경고가 남는다. 직접 ComputeBuffer를 만드는 게임 코드는 없지만 발생 주체와 장기 영향은 미확정이다. 이를 숨기거나 무경고 실행이라고 보고하지 않는다.
- 초기 Unity 승인 실행의 라이선스 서명/handshake·Mono abort/debugger 종료 메시지는 로그에 보존했다. Import/테스트/빌드 완료와 관련 명령 exit0은 별도로 확인했다.
- FPS60 목표를 실측 달성했다고 주장하지 않는다. PNG 연속 저장은 프레임 시간에 영향을 준다. 장시간 성능·여러 해상도·다른 PC·게임패드·배포 설치·오디오 품질은 이번 검증 대상이 아니다.
- 최종 아트 승인·원본 권리/팀 합의·사방신 대응은 기획상 미정이다. 자동 검사는 손맛·직관성·재미를 승인하지 않는다.

## 원본 보존과 Unity 자동 저장

기존 사용자 요청 문서4개, 패키지 안내, 원본5장/manifest, AGENTS/Skill과 기존 SampleScene·Packages를 보존했다. SHA 비교와 Git 최종 기록은 RUN의 preservation/final 파일에 남긴다. Git add/commit/push/브랜치 변경을 하지 않았다.

의도적 게임 변경은 `Assets/ElementLab/**`와 그 meta, 허용된 scope/validation/art/handoff/README, 작은 `tools/element-lab/` 파일이다. 아래는 Unity가 실행·테스트·빌드 중 자동 저장한 별도 변경으로, 수동 롤백하지 않았다. 전체 내용은 `unity-auto-diff.txt`와 마지막 diff를 참조한다.

| 파일 | 확인된 자동 diff |
| --- | --- |
| Assets/Settings/DefaultVolumeProfile.asset | filter 직렬화 필드 추가 |
| Assets/Settings/UniversalRP.asset | 빌드 시 shader prefilter 플래그 갱신 |
| Assets/Settings/UniversalRenderPipelineGlobalSettings.asset | shader stripping 관련 참조/직렬화 필드 |
| ProjectSettings/ProjectSettings.asset | 최종 diff는 Standalone batching 필드 추가. 빌드 도중 기존 InputSystem_Actions preloaded 참조도 관찰됐으나 Unity의 빌드 후 정리로 최종 diff에는 없음 |
| ProjectSettings/UnityConnectSettings.asset | m_Enabled 0→1 자동 저장. 수동 계정/서비스 연결은 하지 않았으며 다른 기존 서비스 플래그는 diff로 보존 |
| Assets/Resources/** | 테스트 프레임워크의 PerformanceTestRunInfo/Settings와 meta |
| ProjectSettings/SceneTemplateSettings.json | Editor 생성 기본 scene template 설정 |

Unity 버전·manifest·packages-lock의 변경은 없다. Git의 LF/CRLF 경고는 컴파일 실패와 구분하고 stderr에 따로 보존한다.

마지막 비교: 시작 파일69개 중61개 해시 동일, 누락0, 승인된 문서3개와 위 Unity 자동 저장5개만 변경. 원본5장은 SHA-256 모두 동일하다. 브랜치/HEAD 유지, staged 변경 없음, `git diff --check` exit0, 누락 meta0. 빌드 폴더180개 파일/107,765,380 bytes의 파일별 SHA-256은 `final-build-files.json`에 기록했다. 검증 프로세스 잔존0을 확인했다. 이 통계는 미추적 신규 파일을 숨기지 않으며 전체 `final-status.txt`와 함께 본다.
