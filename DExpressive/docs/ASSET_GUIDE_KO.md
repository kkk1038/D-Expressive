# 에셋 위치 안내 — v0.2.2

Unity에서 DExpressive 프로젝트를 열어 Assets 아래 폴더를 확인하세요. 파일은 Git 저장소의 DExpressive/Assets에 포함되어 있습니다.

| 구분 | 프로젝트 내 위치 |
|---|---|
| 벽·바닥·천장·창문·침대·책상·의자 등 | Assets/ThirdParty/InteriorKitV1 |
| 욕실·냉장고·신발·음식·전화·쓰레기통 및 새 소품 프리팹 | Assets/ThirdParty/AdditionalProps |
| 약봉투 원본 및 추출 텍스처 | Assets/ThirdParty/DeskMedicine |
| 종이 더미·약봉투 게임용 프리팹 | Assets/ThirdParty/AdditionalProps/GamePrefabs |
| 실제 사용 에셋 참조 | Assets/Settings/InteriorKitGamePalette.asset, AdditionalRoomPropsPalette.asset |
| 배치·크기·회전·Collider·상호작용 연결 | Assets/Scripts/Environment/AdditionalRoomPropsPalette.cs 및 InteriorKitGamePalette.cs |
| 오디오·한글 글꼴·런타임 재질 | Assets/Resources |

URP용 재질, 메시·텍스처 및 Unity .meta GUID를 함께 보관합니다. 에셋 ZIP은 이 프로젝트에 병합할 수 있는 보조 자료이며 단독 실행 파일이 아닙니다. 소스 전체를 사용할 때 에셋 ZIP을 별도로 덮어쓸 필요가 없습니다.

에셋 전체 경로·크기·SHA256은 ASSET_INVENTORY.csv, 참조 점검 결과는 ASSET_REFERENCE_CHECK.json에 있습니다. 기존 CuteMagic 샘플의 미해결 참조는 별도 구분하며 현재 게임 팔레트와 구분합니다. 외부 에셋 원문 라이선스를 보존했으며 THIRD_PARTY_NOTICES.md를 함께 확인하세요.
