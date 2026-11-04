# 에셋 구성과 적용 안내

## 어떤 ZIP을 열어야 하나요?

| 배포 파일 | 포함 내용 |
| --- | --- |
| DExpressive-GitHub-Source.zip | Unity에서 편집할 수 있는 Assets 전체, Packages, ProjectSettings, 문서 |
| DExpressive-Complete-v0.1.0.zip | 위 소스와 Windows 게임 실행 파일 |
| DExpressive-Windows-x64.zip | 실행용 게임 데이터. 원본 프리팹을 개별 편집하는 용도가 아님 |
| DExpressive-Assets.zip | 기존 프로젝트에 사용할 Interior Kit와 추가 소품, 관련 Palette·재질, 에셋 안내·목록 |

별도 에셋 ZIP은 폴더 구조와 .meta를 유지합니다. UnityPackage 형식이 아니라 원래 Assets 하위 구조를 보존한 ZIP입니다. 소스 ZIP을 쓰는 경우 에셋 ZIP의 내용은 이미 포함돼 있습니다.

## 본편이 실제 사용하는 프리팹

Interior Kit 루트: `Assets/ThirdParty/InteriorKitV1/Prefabs/`

| 용도 | 파일 | 상태 |
| --- | --- | --- |
| 벽 | wall_2x3.prefab | 본편 벽과 욕실 입구 고정 부분 |
| 바닥·천장 | floor_2x2.prefab, ceiling_2x2.prefab | 본편 타일 배치 |
| 창문 | window_1200x1400.prefab | 기존 창문 상호작용 연결 |
| 문 | door_interior.prefab | 현관문과 욕실 문 |
| 침대 | bed_double.prefab | 기존 잠자기 기능 |
| 책상 | office_desk.prefab | 기존 책상 위 물건 위치에 맞춰 크기 조정 |
| 의자 | office_chair.prefab | 요청한 180도 회전과 위치 정렬 |
| 러그 | rug_2x3.prefab | 본편 바닥 |
| 책장·소파·싱크대 등 | bookshelf, sofa_3seat, kitchen_counter_sink 등 | 에셋 보유. 본편 배치 여부는 Palette 코드를 기준으로 구분; 별도 InteriorKitTest에서도 확인 가능 |

추가 소품 루트: `Assets/ThirdParty/AdditionalProps/GamePrefabs/`

| 용도 | 프리팹 | 제공 파일 | 기능 |
| --- | --- | --- | --- |
| 식사 | Bento.prefab | free-japanese-bento-box-3d-food-model.zip | 식사·소모·다음 날 복원 |
| 휴대폰 | Phone.prefab | samsung-phone.zip | 날짜별 연락·답장 |
| 신발 | Shoe.prefab | nike-tc-7900-sail.zip | 신발장 앞 한 쌍 |
| 냉장고 | Fridge.prefab | refrigerator-game-ready-kitchen-appliance.zip | 가구. 식사 기능 없음 |
| 쓰레기통 | TrashBin.prefab | plastic_trash_bin_free_low_poly.glb | 신발장과 현관문 사이. 정리 기능 |
| 세면대 | Vanity.prefab | bathroom-asset-part-1.zip | 씻기 |
| 거울 | Mirror.prefab | bathroom-asset-part-1.zip | 거울 선택지 |
| 비누·매트 | HandSoap.prefab, BathMat.prefab | bathroom-asset-part-1.zip | 장식 |
| 변기 | Toilet.prefab | bathroom-asset-part-2.zip | 장식 |
| 휴지걸이·변기솔 | PaperHolder.prefab, ToiletBrush.prefab | bathroom-asset-part-2.zip | 장식 |
| 샤워 공간 | Shower.prefab | bathroom-asset-part-3.zip | 씻기 기록 공유 |
| 욕조 | Bathtub.prefab | bathroom-asset-part-4.zip | 씻기 기록 공유 |
| 수건·선반 | TowelRail.prefab, Shelf.prefab | bathroom-asset-part-4.zip | 장식 |
| 세탁기 | WashingMachine.prefab | bathroom-furniture-set-game-ready.zip | 복합 FBX에서 분리, 장식 |

## 메시·텍스처·재질

- Interior Kit: FBX, 텍스처와 URP 재질, 생성 메시 및 프리팹을 보유합니다.
- 추가 GLB/glTF: 정적 메시를 Unity `.asset`로 변환했습니다. 게임 실행에 GLB 로더나 Python이 필요하지 않습니다.
- `PreparedInput`에는 변환에 사용한 텍스처와 manifest, meshdata가 있습니다. 게임용 프리팹은 `Meshes`와 `Materials`의 Unity 에셋을 참조합니다.
- `ImportedPrefabs`는 분리·검토 단계 프리팹, `GamePrefabs`는 크기·충돌을 정리한 실제 배치용 프리팹입니다.
- URP Lit 재질, 최대 2048px 텍스처와 Unity용 금속도·AO·부드러움 채널을 사용합니다.
- `RuntimeRoom.mat`은 커튼·컴퓨터 등 기존 단순 소품과 날짜별 소품용입니다. 이를 빼면 실행 파일에서 분홍색 재질 문제가 재발할 수 있습니다.

원본 해시 기록은 `AdditionalProps-sources.json`에 있습니다. 배포본에는 사용자가 제공한 모든 ZIP 원본을 다시 넣지는 않았습니다. FBX가 없는 변환 모델을 누락으로 오해해 임의의 대체 모델로 바꾸지 마세요.

## 배치·기능을 수정할 곳

1. 게임 시작 씬은 `Assets/Scenes/SampleScene.unity`입니다.
2. `PrototypeRoomBootstrap`이 원래 방을 생성합니다.
3. `InteriorKitGamePalette.Apply`가 지정된 벽·가구를 교체하고 욕실 문·컴퓨터 방향을 구성합니다.
4. `AdditionalRoomPropsPalette.Apply`가 추가 소품을 배치하고 기존 행동과 연결합니다.
5. `RoomActionVisuals`가 식사·정리 등 기록에 맞게 표시를 갱신합니다.

Play 중 생성된 물건을 Inspector에서 옮기기만 하면 종료 후 사라집니다. 영구 배치는 위 Palette 코드에서 수정하세요. 독립 테스트용 자동 생성기를 본편에 추가하지 마세요.

## 에셋만 다시 넣을 때

먼저 기존 프로젝트를 백업하고 Unity를 닫습니다. 에셋 ZIP 안의 `Assets`를 기존 프로젝트의 `Assets`에 합칩니다. 프로젝트 안에 Assets/Assets가 중첩되지 않게 하세요. `.meta`를 함께 유지하고 GUID를 새로 생성하지 마세요. 코드와 Palette 구조는 최신 소스 ZIP과 일치해야 합니다. 신규 프로젝트를 준비할 때는 에셋 ZIP만 사용하는 대신 소스 ZIP 전체를 여는 것이 확실합니다.

## 목록·연결 검증

`ASSET_INVENTORY.csv`는 경로·종류·크기·GUID·SHA-256 목록입니다. `ASSET_REFERENCE_CHECK.json`은 두 Palette에 연결된 프리팹과 모델·재질 참조의 로컬 존재 여부를 기록합니다. 실행 시 기능 검증 327개와는 별도의 파일 검사입니다.

목록에는 기존 CuteMagic과 Resources/Prefabs/Settings도 포함합니다. 해시는 배포 작업 폴더의 파일 바이트 기준이므로 Git의 텍스트 줄바꿈 변환 뒤에는 텍스트 파일 해시가 달라질 수 있습니다. Unity의 연결 확인은 GUID와 참조 보고서를 함께 사용하세요.

기존 원본 `CuteMagic_Free` 샘플 재질에는 미해결 셰이더·텍스처 GUID 참조 39건이 남아 있습니다. 새 ThirdParty 에셋과 Palette의 참조 누락과 구분하여 보고서에 기록했습니다. 기존 샘플 원본은 보존하며, 본편의 소품은 앞서 정리한 URP 재질을 사용합니다. 샘플 전체를 자동으로 재질 변환하거나 새 파일이라고 주장하지 않습니다.

## 이용 조건

`THIRD_PARTY_NOTICES.md`를 유지하세요. 제공 모델의 원 배포 URL·제작자·재배포 조건 중 미확인 항목이 남아 있습니다. 비공개 제출 저장소의 공개 설정을 임의로 바꾸지 않습니다.
