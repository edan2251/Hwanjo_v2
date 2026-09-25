# Element Lab v0.1 아트 기록

2026-09-25. 현재 게임용 신규 리소스이며 팀의 최종 아트 승인은 별도다. 사용자 원본과 요청 문서는 수정하지 않았다.

## 입력과 도구

실제 입력은 루트 `element-lab-v01/`의 원본 PNG 5장이다. 안내된 `references/element-lab-v01/` 경로는 이 저장소에 없으므로 원본을 옮기지 않고 사용했다. 5장 모두 `view_image`로 실제 열었으며 시작 해시와 원본 manifest 일치를 확인했다.

| ID | 원본 파일 | 실제 반영 |
| --- | --- | --- |
| REF01 | Hwanjo_Reference_Character.png | 묶은 검은 머리, 붉은 스카프, 남색/아이보리 의상, 작은 금색 장식, 같은 인간 주인공 |
| REF02 | Hwanjo_Reference_Stage1.png | 작은 인물과 넓은 숲 화면, 청록 숲·이끼 유적, 전경 분리 |
| REF03 | Hwanjo_Reference_4Concept.png | 숲 배경의 동양풍 정자·석문·원경 깊이감 |
| REF04 | Hwanjo_Reference_VFX1.png | 밝은 불 호/불티, 단타의 준비·접촉·회수 대비 |
| REF05 | Hwanjo_Reference_VFX2.png | 냉색 참격 대비와 각진 파편, 큰 타격의 자세 차이 |

쌍검·대도·3타·백발 폼·호랑이·사방신 소환은 사용하지 않는다. 물/바람 VFX는 원본 완성본의 복제가 아닌 명세의 신규 제안이다.

내장 `image_gen.imagegen`을 4회 사용했다(상한8회). 동일 산출물: Run/Slash 1회, 나머지 동작2회, 배경1회. 유료 외부 API/CLI·새 도구 설치는 하지 않았다. 전체 프롬프트와 선택/탈락 생성본은 `artifacts/element-lab/20260924T232704+0900/art-prompts.md`, `art-source/`에 있다. 기본 생성 저장 경로의 파일만 참조하는 자산은 없다.

## 채택 파일

아래 경로는 `Hwanjo_v2_Test/Assets/ElementLab/` 기준이다.

| 파일 | 규격·사용 | 제작/가공 |
| --- | --- | --- |
| Art/HeroRunSlash.png | 288×96 RGBA, 6×2, 셀48×48. Run6/Slash6 | 생성본2172×724의 12개 포즈를 균등 셀로 분리, 공통 배율 nearest 축소, 발 기준 정렬. 원본의 문구/캐릭터를 잘라 쓴 것이 아님. |
| Art/HeroOtherPoses.png | 288×192 RGBA, 6×4, 셀48×48 | 두 번째 생성본1536×1024. 원본 행 경계0/300/554/815/1024를 실측 분리. alpha128 기준 이진화로 낮은 알파 잔상을 제거, 같은 배율로 최대 높이40px, 발 정렬. |
| Art/ForestRuins.png | 1672×941 RGB 불투명 숲 유적 배경 | REF02/03 기반 신규 생성. 카메라 뒤에 표시하며 배경 그림에 물리 충돌을 부여하지 않음. |
| Data/LabArt.asset | Sprite 참조, 10종 재생 스트립 | Editor/LabArtImport.cs가 import와 명시적 프레임 대응. |
| Scripts/LabArtLibrary.cs | 환경6종/더미/적의 코드 픽셀 그래픽 | 신규 PixelCanvas 도형. 주인공 placeholder는 자산 누락 시 안전 대체용으로 남지만 배포 씬은 생성 스프라이트를 사용. |
| Scripts/LabWorld.cs, TraceSlot.cs | 검1개, 속성 호·입자·검흔·발판 | 런타임 기하 형태. 불/밝은 호·불티, 물/아래로 떨어지는 방울, 바람/방향선, 얼음/회전 결정과 발판. 무기/FX를 몸체 셀과 분리. |

모든 몸체 Sprite: PPU48, pivot(24,2 px), Point, 무압축, mipmap OFF, Sprite Multiple. 발은 셀 y=46(top origin) 아래에 2px 여백. 프레임마다 크기를 따로 늘리지 않는다. 좌우 이동에는 같은 원형을 반전한다.

| 동작 | 순서(source index, 0 기반) | 수 |
| --- | --- | --- |
| Run / Slash | RunSlash 0–5 / 6–11 | 6 / 6 |
| Idle / Jump / Fall | Other 0–3 / 4 / 5 | 4 / 1 / 1 |
| Dash / Charge / Hit | Other 6,7 / 8,9 / 10,11 | 2 / 2 / 2 |
| ChargeRelease | Other 8,9,13,15,16,17 | 6 |
| Death | Other 18–23 | 6 |

생성 셀36개 중 서로 다른34개를 사용하고, Charge의 준비2프레임을 ChargeRelease에서도 공유한다. 공격의 준비/접촉/회수에 각각2프레임을 대응한다. 같은 주인공의 준비 자세를 공유하며 다른 폼이나 추가 공격을 만들지 않는다.

## 검수와 한계

- 첫 Run/Slash는 다음 동작 생성 전에 48px 원래 크기와4배 확대를 실제 열어 다리 교대와 몸통/팔의 변화를 확인했다.
- 두 번째 시트는 표시된 배경 잔상과 불균등 행 때문에 수정 요청1회를 사용했다. 최종 alpha 수치를 검사하고 청록 바탕에 알파 합성한 확대본을 실제 열어 윤곽을 확인했다. 단순 RGB 미리보기와 최종 알파 합성을 구분했다.
- 가공은 `tools/element-lab/slice_sprites.py`의 셀 분리/같은 배율 축소/발 정렬/알파 처리다. 다른 동작을 같은 이미지의 좌표 이동만으로 만들지 않았다. 각 가공 프레임의 해시·원본 bounds는 `art-source/*.slicing.json`에 있다.
- 기술적 import 검증과 실제 게임 화면 검수 결과는 `PROTOTYPE_VALIDATION.md`에서 구분한다. 최종 미술 품질·동작 손맛·사방신 속성 대응은 사용자/팀 판단을 기다린다.
- 최종 배포 씬은 10종 모든 몸체 동작에 생성 Sprite를 연결했다. Import 검사33개 중 아트 검사3개 통과. 실제 Windows 재생에서 Idle/Run/Slash/Charge/ChargeRelease/Dash/Death, 직접 키 입력에서 Jump/Fall, 적과의 실제 접촉에서 Hit를 확인·저장했다. 몸체 placeholder를 쓰는 누락 동작은 없다.
- 최종 게임 캡처와 확대 표본의 파일 경로·직접 입력/자동 입력 구분은 검증 문서에 있다. ChargeRelease와 Run 각6포즈, 적 동결, 피격과 더미 UI PNG를 실제로 열어 보았다. 아트 최종 취향 승인·장시간 애니메이션 손맛 평가는 미완료이며 기술적 연결 완료와 구분한다.
