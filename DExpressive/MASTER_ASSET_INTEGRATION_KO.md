# 최종 master 코드에 에셋 적용

기준: 사용자가 제공한 `D-Expressive-master.zip` / 적용일: 2026-11-05.

이 작업본은 이전 제출 프로젝트를 최종 코드 위에 통째로 덮어쓴 버전이 아닙니다. 제공 ZIP을 별도 폴더에 풀고 에셋 배치에 필요한 부분만 병합했습니다.

## 적용 내용

- Interior Kit v1의 벽·바닥·천장·창문·문·침대·책상·의자·러그.
- 욕실 세면대·거울·샤워 공간·욕조·변기·비누·매트·수건·선반·세탁기.
- 도시락·휴대폰·신발·냉장고·플라스틱 쓰레기통.
- 쓰레기통은 신발장과 현관문 사이, 의자는 180도·모니터는 90도 회전한 배치.
- 식사 기능은 도시락에만 있고 냉장고는 가구입니다.
- 욕실 문은 F키로 열기·닫기. 닫힌 문 충돌 및 플레이어 끼임 방지. 날짜별 현관문 이야기와 분리합니다.
- 먹은 음식과 정리한 내용물 표시를 최신 선택 기록에 연결하며 다음 날·불러오기·회귀에 맞춰 복원합니다. 3일차 순서형 `_meal_1/2/3` 기록도 구분합니다.
- 기존 단순 소품에는 명시적인 URP 재질을 사용합니다. 배포용 한글 글꼴은 OFL 1.1의 나눔고딕입니다.
- 기존 기상 시작점이 새 침대 Collider와 겹쳐, 시작 좌표만 침대 옆 `(-1.0, 0.05, 3.15)`로 조정했습니다. 기상 카메라 연출·방향·이동 속도 규칙은 유지합니다.

## 최종 코드에서 유지한 부분

- 날짜별 필수 목표와 체크리스트, 목표 미완료 시 수면 차단.
- 3일차 행동 순서, 4일차 쓰러짐, 5~6일차 행동 지연과 정신력별 이동 속도.
- 정신력·회복 상태별 BGM 및 휴대폰·식사·씻기·현관문 효과음.
- 기상 카메라 연출, 7일차 연락 회복, 최신 대사·선택지와 엔딩.
- 기존 DayManager·StoryContentFactory·DialogueDatabase·Audio·저장·체크포인트·회귀 코드.
- 대응 모델이 불명확한 옷·메모·컴퓨터·화분·약 등의 기존 물건.

최종 master는 9일차 현관문에서 기다리는 선택지를 사용합니다. 이전 에셋 제출판의 9일차 문 개방 규칙으로 되돌리지 않았습니다. 10일차 밖으로 나가기 엔딩을 유지합니다.

원본의 StoryContentFactoryTests 한 항목은 실제 최종 코드와 달리 이전 `day9_open_door`를 기대하고 있었습니다. 게임 대사는 바꾸지 않고 테스트의 기대값만 최종 코드의 `day9_door_wait`로 갱신했습니다. 원본 테스트는 Backups/BeforeMasterAssetIntegration에 보관했습니다.

## 에셋 위치

| 찾는 것 | 경로 |
| --- | --- |
| 벽·가구 프리팹 | Assets/ThirdParty/InteriorKitV1/Prefabs |
| 추가 소품 프리팹 | Assets/ThirdParty/AdditionalProps/GamePrefabs |
| 추가 메시·재질·텍스처 | Assets/ThirdParty/AdditionalProps/Meshes, Materials, PreparedInput |
| 본편 배치 참조 | Assets/Settings/InteriorKitGamePalette.asset, AdditionalRoomPropsPalette.asset |
| 배치 코드 | Assets/Scripts/Environment/InteriorKitGamePalette.cs, AdditionalRoomPropsPalette.cs |

실행 시 방이 생성되므로 Play 중 이동한 Transform은 영구 변경이 아닙니다. 지속되는 배치는 Palette 코드에서 수정합니다. `.meta`와 GUID를 함께 유지하세요.

## 실행

Unity Hub에서 이 폴더를 Unity **6000.3.22f1**로 열고 `Assets/Scenes/SampleScene.unity`를 실행합니다. URP 17.3.0을 사용합니다. `InteriorKitTest`는 별도 모델 테스트 씬입니다.

Windows 실행 ZIP은 모두 압축 해제하고 `DExpressive.exe`를 실행합니다. EXE와 Data·DLL·MonoBleedingEdge 폴더를 함께 유지합니다. WASD 이동, 마우스 시점, F 조사·문 개폐, 클릭/1/2 선택, Esc 메뉴, F5 저장, F9 불러오기입니다.

빌드는 Unity의 DExpressive → Build Windows Release 메뉴를 사용합니다. 결과는 Builds/Windows입니다. `FinalMasterSetup.ApplyAndBuild`는 현재 SampleScene에 Palette 참조를 연결하고 빌드합니다.

검증 결과는 `FinalMasterAssetValidation.txt`, 빌드 결과는 `FinalMasterWindowsBuild.txt`, 원본 파일 비교는 `MasterPreservation.json`을 확인하세요.

## 최종 검증

- 버전 0.2.0, Unity 6000.3.22f1, Windows x64 / Mono.
- 최종 Windows 빌드 성공, 오류 0·경고 0.
- 실제 EXE를 직접 실행하여 제목·한글·새 게임·1일차 목표 체크리스트·시작 대화 선택과 방·새 소품의 정상 표시를 확인했습니다.
- 새 에셋과 최신 master 규칙의 Play 모드 자동 검사 **347개 PASS**.
- 기존 스토리 Edit Mode 테스트 **12개 PASS**, 실패 0. 상세 XML: `MasterEditModeResults.xml`.
- 원본 ZIP 파일 728개가 동일합니다. 핵심 진행·대사·오디오·저장·기상·엔딩 관련 보호 대상 58개 파일은 바이트 기준으로 모두 동일합니다.
- 원본에서 변경된 파일은 17개입니다. 에셋 연결, 창문 재질 연출, 소품 표시 복원, 기상 위치, 한글 글꼴, Unity 설정·안내문과 이전 기대값을 가진 테스트 항목이 포함됩니다.
- Play 모드에서 목표 미완료 수면 차단, 3일차 순서형 행동, 4일차 쓰러짐과 날짜 전환, 7일차 회복 플래그·정신력 보호, 저장·불러오기, 다음 날 표시 복원, 9일차 대기, 10일차 엔딩, 회복 이전 분기의 정신력 소진 회귀를 확인했습니다.
- 욕실 문 양쪽 충돌·열림 통과·끼임 방지와, 문 조작이 스토리 체크리스트를 바꾸지 않는 것을 확인했습니다.
- BGM 4개와 효과음 4개의 로딩 및 기존 오디오 컨트롤러 유지를 검사했습니다. 모든 음원을 실제로 끝까지 청취하거나 다른 PC의 오디오 출력을 검증한 것은 아닙니다.
- 에디터 검색 인덱스의 시작 예외가 기록된 실행에서도 검사기가 일시정지를 해제한 뒤 게임 검사를 완료했습니다. 에디터 내부 예외와 게임 검증 결과를 구분합니다.

자동 진행 검사에는 필요한 게임 상태 설정이 포함됩니다. 모든 선택 조합의 수동 완주나 다른 PC 성능·호환성 검사는 별도입니다.

`FinalMaster-Room.png`, `FinalMaster-Bathroom-Closed.png`, `FinalMaster-Bathroom-Open.png`는 통합된 본편의 Unity 카메라 렌더입니다.

외부 모델의 출처·이용 조건은 `THIRD_PARTY_NOTICES.md`에 기록합니다. 원본 ZIP은 그대로 보존했고, Unity 캐시와 개인 저장 파일은 전달 압축본에서 제외합니다.
