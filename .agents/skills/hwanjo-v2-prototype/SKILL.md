---
name: hwanjo-v2-prototype
description: Guide implementation, changes, and regression checks for the Hwanjo v2 Unity prototype in this repository, preserving its agreed scope and recording actual validation. Use for authorized v2 prototype work; do not activate for general conversation or v1 form combat, and do not treat this skill as permission to start game implementation during Step 0.
---

# Hwanjo v2 프로토타입 작업

아래 문서 경로는 저장소 루트 기준이다. Unity 작업 대상은 `Hwanjo_v2_Test/`다. 이 Skill은 지침형 하나로 유지한다.

## 작업 전 문서와 변경 확인

- [AGENTS.md](../../../AGENTS.md), [docs/HANDOFF.md](../../../docs/HANDOFF.md), [docs/ENVIRONMENT_AUDIT.md](../../../docs/ENVIRONMENT_AUDIT.md)를 읽고 현재 브랜치·사용자 변경·검증 차단 사유를 확인한다.
- 기획과 숫자는 [docs/PROTOTYPE_SCOPE.md](../../../docs/PROTOTYPE_SCOPE.md)만 기준으로 삼는다. 확정·추가 요청·추천·미정을 구분하고 별도의 기획 원본을 만들지 않는다.
- 사용자 요청에서 이번 단계의 허용 범위를 확인한다. Step 0 요청이면 환경·지침·문서 준비 후 멈춘다. 구현 요청 없이 코드·씬·더미·적·아트 제작을 시작하지 않는다.

## 지정 범위 작업

- 허가된 v2 범위만 최소 변경한다. 미정사항이 구현을 좌우하면 결정이 필요한 부분을 명시하고 해당 결정을 임의 확정하지 않는다. 이미 확정한 기준은 다시 묻지 않는다.
- 기존 환조 v1 저장소나 전투 프로토타입을 찾거나 복사하거나 수정하지 않는다. 캐릭터 레퍼런스가 없으면 없는 상태로 기록한다.
- 사용자 변경, 기존 `.meta`와 GUID, 패키지 및 설정을 보존한다. Unity가 추적 파일을 자동 변경하면 목록과 diff를 남기고 임의로 원복하지 않는다.
- 거대한 런처, 야간 실행기, 병렬 에이전트, 스크립트 묶음을 만들지 않는다.

## 실제 검증

- 최신 환경 보고서의 Editor 경로와 프로젝트 버전을 확인한다. 프로젝트 파일 쓰기 권한과 Unity 실행 권한은 별개로 확인한다.
- 해당 프로젝트가 Editor에서 열려 있으면 사용자에게 정상 종료를 요청한다. 다른 작업의 프로세스를 종료하거나 잠금 파일·캐시를 삭제하지 않는다.
- 요청에 필요한 검증만 실행한다. 절대 경로 또는 명확한 작업 디렉터리를 쓰며 .NET 파일 I/O에는 절대 경로를 전달한다. Git 파일 목록 stdout과 stderr 경고를 분리한다.
- Import/컴파일 검증 성공은 실제 Unity 실행, 종료 코드 0, 로그의 완료·정상 종료 확인 및 미해결 Import/컴파일 오류 없음으로 판정한다. 미실행은 미검증, 비정상 종료·관련 오류는 실패로 기록한다. 단순 경고만으로 실패라고 단정하지 않는다.
- 실행이 권한·라이선스·샌드박스에서 차단되면 최초 원인과 필요한 해당 명령의 승인을 기록한다. 허용되지 않으면 BLOCKED다. 같은 오류를 반복 시도하거나 전역 권한·라이선스·설정을 바꾸지 않는다. 오류 무시 옵션이나 버전·패키지 변경으로 통과시키지 않는다.
- 결과에 실제 명령, stdout/stderr, Unity 전체 로그, 종료 코드를 구분해 남긴다. 진단 파일은 `artifacts/setup/<실제 실행 식별자>/`에 두며 비밀정보는 제거 사실을 표시하고 가린다.
- 테스트가 없으면 `테스트 0개 / 미작성`으로 기록한다. Import/컴파일, 프로젝트 테스트, 플레이·UI·렌더링, 빌드, 성능, 이미지 도구 노출과 실제 생성 결과를 각각 구분한다. 하나의 성공으로 나머지를 통과 처리하지 않는다.

## 결과와 미정 기록

- 환경 변경과 실제 결과는 `docs/ENVIRONMENT_AUDIT.md`, 이번 변경·남은 문제·다음 시작점은 `docs/HANDOFF.md`에 갱신한다. 기획 변경은 사용자 결정 근거와 함께 `docs/PROTOTYPE_SCOPE.md`에만 반영한다.
- 실행하지 않은 명령은 예정/미실행으로 표시한다. 차단된 검증을 성공으로 기록하지 않는다.
- diff 요약과 Git 상태를 보고하고 요청한 단계에서 멈춘다. 사용자 요청 없는 Git 기록·원격 변경은 하지 않는다.
