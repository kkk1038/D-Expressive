# 클로드 인계용 프롬프트

아래 내용을 새 Claude 대화에 붙여 넣고 최신 소스 ZIP을 함께 제공하거나, 저장소 접근을 연결하세요. 실행 전용 ZIP만 주면 소스 수정은 할 수 없습니다. 마지막의 후속 작업 부분은 필요에 맞게 바꿔 사용하세요.

---

너는 Unity 게임 프로젝트 D-EXPRESSIVE의 후속 개발 담당자다. 첨부 소스 또는 `kkk1038/D-Expressive` 저장소의 실제 최신 내용을 먼저 읽고 작업해라. 이 설명만으로 파일이 존재하거나 업로드가 끝났다고 가정하지 말고 확인해라. 저장소는 비공개 제출 공간이며 공개 설정을 바꾸지 마라.

## 프로젝트와 실행 환경

- Unity 6000.3.22f1 / URP 17.3.0 / Windows x64 / Mono.
- 저장소에서는 `DExpressive/Assets`, `DExpressive/Packages`, `DExpressive/ProjectSettings`가 Unity 프로젝트다. 프로젝트 루트 형태 ZIP이면 Assets가 보이는 폴더를 사용해라. 경로 중첩을 먼저 확인해라.
- 본편 시작 씬: `Assets/Scenes/SampleScene.unity`.
- `InteriorKitTest`는 별도 에셋 테스트 씬이며 본편이 아니다.
- 게임은 방 안에서 작은 행동과 선택으로 열흘을 보내는 1인칭 심리 서사 프로토타입이다.
- WASD 이동, 마우스 시점, F 조사, 클릭/1/2 선택, Esc 메뉴, F5 저장, F9 불러오기. 침대에서 다음 날로 진행한다.

## 반드시 보존할 기존 기능

- DAY 1~10, 날짜별 대화·선택·목표·방 변화.
- 정신력, 하루 행동 기록, 단일 슬롯 저장·불러오기, 날짜별 체크포인트·회귀.
- 정신력이 0이면 이전 날짜 시작 체크포인트로 복원. DAY 1은 DAY 1로 복원.
- 행동 없이 잠들면 정신력 30 감소.
- DAY 9 현관문 열기, DAY 10 밖으로 한 걸음 선택과 엔딩 화면.
- 약·컵·옷·메모·옷장·화분 등 대체 모델을 특정할 수 없는 기존 물건은 임의로 바꾸거나 삭제하지 않는다.

## 현재 에셋 및 배치 상태

- `Assets/ThirdParty/InteriorKitV1`: 벽·바닥·천장·창문·문·침대·책상·의자·러그 등의 모델·텍스처·URP 재질·프리팹.
- `Assets/ThirdParty/AdditionalProps/GamePrefabs`: Bento, Phone, Shoe, Fridge, TrashBin, Vanity, Mirror, Shower, Bathtub, Toilet, PaperHolder, ToiletBrush, TowelRail, HandSoap, BathMat, Shelf, WashingMachine.
- 추가 GLB/glTF는 Unity 네이티브 메시 `.asset`와 프리팹으로 변환됐다. FBX가 없다는 이유만으로 누락이라 판단하지 마라. `PreparedInput`에 변환 데이터·텍스처가 있다. 플레이에 Python이나 GLB 임포터는 필요 없다.
- 기존 CuteMagic 샘플 원본 재질에는 미해결 셰이더·텍스처 GUID 39건이 있다. 새 ThirdParty·Palette 참조 누락과 구분한 보고서를 읽어라. 기존 샘플 원본을 본편 사용 에셋으로 오해해 일괄 변환하지 마라.
- 쓰레기통은 신발장과 현관문 사이 `(2.4, 0, -1.05)`에 있다. 정리하면 컵·쓰레기통 내용물만 숨기고 통은 남긴다. 다음 날과 저장 복원 시 기록에 맞게 표시한다.
- 의자는 이전 배치에서 180도 회전하여 `(2.65, 0, 4.05)`, yaw 0. 모니터·화면은 그룹 yaw -90, 위치 `(2.65, 1.48, 5.85)`, 키보드는 `(2.65, 1.18, 5.1)`이다. 의자를 바라보는 화면 방향을 유지해라.
- 식사는 책상 도시락에서만 한다. 냉장고에는 Meal 상호작용을 추가하지 마라. 먹은 도시락은 사라지고 다음 날 복원된다.
- 세면대·샤워·욕조는 기존 하루 씻기 기록을 공유한다. 거울은 기존 거울 선택지를 사용한다.
- 욕실 문은 F키로 안쪽으로 열고 닫는다. 닫히면 양쪽 통과를 막고, 플레이어가 끼면 멈추며 비키면 닫힘을 계속한다. 이 문은 하루 행동·정신력·현관문 엔딩을 건드리지 않는다. 문 상태는 저장하지 않고 장면 시작 시 닫힌다.

## 수정할 코드와 자료

- 방 생성: `Assets/Scripts/Environment/PrototypeRoomBootstrap.cs`.
- 실내 교체·문·컴퓨터: `InteriorKitGamePalette.cs`와 `Assets/Settings/InteriorKitGamePalette.asset`.
- 추가 소품 배치·기능: `AdditionalRoomPropsPalette.cs`와 `AdditionalRoomPropsPalette.asset`.
- 욕실 문: `BathroomDoorController.cs`.
- 조사·프롬프트: `PrototypeInteractionTarget.cs`, `PrototypeInteractionController.cs`, `Assets/Scripts/Player/FirstPersonPlayerController.cs`.
- 행동 후 표시: `RoomActionVisuals.cs`.
- 상태·날짜·저장: `GameStateManager`, `DayManager`, `CheckpointManager`, `RollbackManager`, `PrototypeSaveManager`.
- 이야기: `DialogueDatabase`, `StoryContentFactory`, `DialogueManager`, `ChoiceManager`.
- 배포용 한글: `KoreanFontBootstrap.cs`, `Assets/Resources/Fonts/NanumGothic-Regular.ttf`, OFL 라이선스.
- 실행 시 단순 소품: `RuntimeRoomMaterial.cs`, `Assets/Resources/Materials/RuntimeRoom.mat`. 기본 Primitive 재질에 의존하면 EXE에서 분홍색이 되는 문제가 있으므로 명시적 URP 재질을 유지해라.

방은 실행 시 생성된다. Play 중 Hierarchy의 Transform만 수정한 뒤 완료라고 하지 마라. Palette 코드 또는 지속되는 에셋에 변경해야 한다. 기존 자동 생성기·테스트 씬 플레이어를 본편에 중복 추가하지 마라. `.meta`와 GUID를 유지해라.

## 검증 및 빌드 상태

- 직전 배포는 Windows 빌드 성공, 오류 0·경고 0.
- `InteriorKitGameVerification` Play 모드 검사 327개 PASS. `docs/validation/editor-final.txt`와 `docs/VERIFICATION_KO.md` 확인.
- 검사에는 기존 행동, 저장·불러오기·회귀, DAY 1~10, 엔딩, 소품 표시 복원, 욕실 문 충돌·통과·끼임, 음식 단독 식사, 요청 배치와 URP 재질 참조가 포함된다.
- 실제 EXE에서는 제목·한글, 새 게임, 대화 버튼과 방 렌더링을 확인했다. 모든 선택 조합의 수동 완주나 다른 PC 성능 검사는 하지 않았다.
- Unity 시작 시 에디터 검색 인덱스 내부 예외가 기록된 적은 있으나 게임 자동 검사는 완료됐다. 게임 예외와 에디터 내부 예외를 구분해라.
- 빌드: Unity 메뉴 `DExpressive > Build Windows Release` 또는 `-batchmode -quit -executeMethod ReleaseBuild.Windows`. 결과 `Builds/Windows/DExpressive.exe`.
- 자동 검사 실행은 `-executeMethod InteriorKitGameVerification.Begin`; 완료 시 에디터 자체가 종료되므로 `-quit`을 붙이지 않는다.
- 실행 파일은 EXE·Data·DLL·MonoBleedingEdge를 함께 묶는다. 개인 저장 파일과 Library/Temp/Logs/UserSettings/DoNotShip 폴더는 배포하지 않는다.

## 먼저 할 일

1. git 상태와 실제 폴더·문서를 확인하고, 위 설명과 다른 점을 보고해라.
2. `docs/ASSET_GUIDE_KO.md`, `ASSET_INVENTORY.csv`, `ASSET_REFERENCE_CHECK.json`을 읽고 Palette의 프리팹·메시·재질 GUID 연결과 실제 에셋을 확인해라.
3. 명확한 누락·깨진 참조가 있으면 기존 파일·기능을 보존하면서 수정해라. 새 모델로 임의 교체하거나 스토리를 재작성하지 마라.
4. 아래 후속 작업을 실제 파일에 반영해라. 후속 작업이 지정되지 않았으면 현재 상태 확인과 발견한 명확한 오류 수정까지만 진행하고, 임의의 새 기능을 추가하지 마라.
5. 변경에 해당하는 검사를 하고 가능하면 Windows 빌드까지 확인해라. Unity나 EXE를 실행할 수 없다면 그 사실을 명시하고 실행 검증했다고 말하지 마라.
6. 바뀐 파일, 유지한 기능, 검사 결과, 남은 문제와 전달 파일을 보고해라. Git 업로드는 사용자가 지정한 저장소·범위 내에서만 하고 기존 이력을 강제 덮어쓰지 마라.

외부 에셋은 `THIRD_PARTY_NOTICES.md`를 따른다. 일부 원 배포처·재배포 조건은 미확인이다. 프로젝트 전체에 MIT 등 라이선스를 임의로 부여하지 마라.

후속 작업: 우선 현재 전달본의 에셋 참조와 본편 실행 상태를 확인하고, 명확히 누락된 파일이나 깨진 연결이 있으면 수정해 줘. 그 외 기능 변경은 내가 추가로 지정하겠다.

---
