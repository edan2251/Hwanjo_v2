# Hwanjo v2 저장소 작업 지침

- Git·문서·Skill의 기준 위치는 저장소 루트이며, Unity 프로젝트는 `Hwanjo_v2_Test/`다.
- 작업 전 현재 브랜치, staged/unstaged/untracked 변경과 `docs/HANDOFF.md`를 읽는다. 사용자 변경을 덮어쓰거나 임의로 원복하지 않는다. 필요한 파일만 최소 변경한다.
- 기획과 수치의 단일 기준은 `docs/PROTOTYPE_SCOPE.md`다. 확정·추가 요청·추천·미정을 구분하며, 미정사항을 사용자 결정으로 바꾸지 않는다.
- 기존 환조 v1 저장소·전투 프로토타입을 찾거나 복사하거나 수정하지 않는다. v1 시스템과 야간 실행기를 임의로 이식하지 않는다.
- 현재 요청은 Step 0 환경·문서 준비다. 후속 구현 요청 전에는 게임 코드·씬·아트를 제작하지 않는다. 단계별 허용 범위를 따른다.
- 검증은 실제 실행한 명령, 종료 코드, 로그와 한계를 기록한다. 미실행·권한 차단·실패를 성공으로 쓰지 않는다. Unity 자동 변경은 목록과 diff를 보고하며 숨기거나 임의로 복구하지 않는다.
- 사용자 요청 없이 Git add/commit/push/merge/switch/reset/restore/clean/stash 또는 원격 변경을 하지 않는다.
- 진단 결과는 `artifacts/setup/`, 빌드 산출물은 `Builds/`에 둔다. 비밀정보·개인 설정 원문을 문서나 로그에 남기지 않는다.

작업 문서:

- `docs/PROTOTYPE_SCOPE.md`: 확정 기준과 미정사항.
- `docs/ENVIRONMENT_AUDIT.md`: 로컬 경로, 환경 조사, 실제 검증 결과.
- `docs/HANDOFF.md`: 이번 결과, 남은 문제, 다음 작업 시작점.
- `.agents/skills/hwanjo-v2-prototype/SKILL.md`: 이 저장소의 v2 구현·수정·회귀 검증 절차. Skill 자체는 구현 시작 허가가 아니다.
