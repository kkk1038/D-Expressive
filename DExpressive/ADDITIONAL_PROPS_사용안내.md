# 추가 에셋 적용 및 기능 안내

현재 프로젝트: IntegratedWork/DExpressive · 실행 씬: Assets/Scenes/SampleScene.unity
Unity 6000.3.22f1 / URP 17.3.0

## 배치와 기능

| 물건 | 위치 | F키 기능 |
| --- | --- | --- |
| 도시락 | 기존 책상 왼쪽 식사 자리 | 기존 식사 선택지, 정신력 변화, 식사 완료 기록 |
| 삼성 휴대폰 | 기존 책상 중앙 | 날짜별 메시지/연락 선택지 |
| 냉장고 | 방 오른쪽, 신발장과 세탁기 사이 | 상호작용 없음 |
| 플라스틱 쓰레기통 | 신발장과 현관문 사이 | 기존 정리 선택지 |
| 세면대 수납장 | 욕실 안쪽 벽 | 기존 세수/씻기 선택지 |
| 원형 거울 | 세면대 위 | 기존 거울 선택지 |
| 샤워 공간 | 욕실 입구 왼쪽 | 기존 세수/씻기 선택지 |
| 욕조 | 욕실 앞쪽 벽 | 기존 세수/씻기 선택지 |
| 욕실 문 | 욕실 입구 | F키로 열기/닫기 |
| 신발 한 쌍 | 신발장 앞 | 환경 오브젝트 |
| 변기·휴지걸이·변기솔 | 욕실 왼쪽 벽 쪽 | 환경 오브젝트 |
| 수건걸이·수건·비누·욕실 매트·선반 | 욕실 내 벽·세면대·바닥 | 환경 오브젝트 |
| 세탁기 | 방 오른쪽, 냉장고와 옷장 사이 | 환경 오브젝트 |

식사는 책상 위 도시락에서만 진행합니다. 세면대·샤워·욕조도 기존 하루 씻기 기록을 공유합니다. 행동은 기존 대화와 선택지 방식으로 진행됩니다.

식사 후 도시락은 숨겨지고 다음 날 다시 나타납니다. 정리를 선택하면 기존 정리 대상인 컵과 쓰레기통 내용물이 사라지고, 쓰레기통 자체는 남아 있습니다. 다음 날에는 내용물도 복원됩니다. 저장/불러오기 및 날짜 회귀 시에도 기존 행동 기록에 맞춰 상태를 복원합니다.

## 제공 파일 사용 내역

- bathroom-furniture-set-game-ready.zip: 복합 FBX를 분리해 세탁기 모델을 사용했습니다. 원본의 샤워 부품 등은 별도 프리팹 자료로 보관합니다.
- bathroom-asset-part-1.zip: 세면대 수납장, 거울, 비누통, 욕실 매트.
- bathroom-asset-part-2.zip: 변기, 휴지걸이, 변기솔.
- bathroom-asset-part-3.zip: 샤워 공간과 내부 용품.
- bathroom-asset-part-4.zip: 욕조, 수건걸이와 수건, 욕실 선반.
- free-japanese-bento-box-3d-food-model.zip: 도시락.
- nike-tc-7900-sail.zip: 신발 한 쌍을 배치할 원본 신발 모델.
- samsung-phone.zip: 휴대폰.
- refrigerator-game-ready-kitchen-appliance.zip: 냉장고.
- plastic_trash_bin_free_low_poly.glb: 쓰레기통.

## 유지한 부분

기존 방의 벽·바닥·천장·침대·책상·의자·창문·현관문 세팅과 Day 1~10, 대화, 선택지, 정신력, 저장/불러오기, 체크포인트/회귀, 엔딩을 유지했습니다.

이번에 대응 모델을 제공받은 기존 휴대폰·식사 블록·세면대·거울·욕실 선반·신발을 교체했습니다. 약·정리할 컵·컴퓨터·키보드·옷장·신발장·옷·메모·커튼·화분 등은 유지했습니다.

욕실 입구에는 새 문틀과 고정 벽을 설치했습니다. 문을 바라보고 F를 누르면 안쪽으로 열리거나 닫힙니다. 문 사이에 플레이어가 있으면 닫힘을 멈추고, 자리를 비우면 계속 닫힙니다. 욕실 문은 실행 시 닫힘으로 시작하며 기존 현관문 엔딩, 날짜, 정신력과는 독립적으로 동작합니다. 샤워 공간은 개구부를 막는 통짜 BoxCollider 대신 구조 메시 충돌을 사용하며, 작은 매트는 충돌을 생략했습니다.

## Unity에서 사용

1. 이 안내문이 있는 DExpressive 폴더를 Unity Hub에서 엽니다.
2. Assets/Scenes/SampleScene.unity를 실행합니다.
3. 새 게임 또는 저장 불러오기를 선택합니다. 첫 대화 뒤 WASD 이동, 마우스 시점 이동, F키 조사, Esc 메뉴, F5 저장, F9 불러오기를 사용합니다.

Assets/ThirdParty/AdditionalProps/GamePrefabs에 크기와 충돌 설정을 마친 프리팹들이 있습니다. 재질은 Materials 폴더의 URP Lit 재질을 사용합니다. 고해상도 텍스처는 최대 2048px로 정리했고, 원본 ORM을 Unity용 금속도/AO/부드러움 채널로 변환했습니다. 큰 glTF 모델은 재질 경계와 UV를 유지하는 방식으로 경량화했습니다. Unity 실행에는 별도의 glTF 패키지나 Python 설치가 필요하지 않습니다.

Assets/Settings/AdditionalRoomPropsPalette.asset에서 새 모델 참조를 확인할 수 있습니다. 기존 방 생성 방식에 명시적 교체 단계를 추가했으므로, 이전 테스트 씬이 아닌 SampleScene을 실행해야 합니다.

## 검증 자료 및 범위

InteriorKitGameValidation.txt에 최종 Play 모드 자동 검증 결과가 있습니다. 새 모델 존재·재질·Collider·욕실 진입 경로, 도시락 소모 및 다음 날 복원, 기존 상호작용, Day 1~10 진행, 정신력, 저장/불러오기, 회귀, 9일차 문 열기와 10일차 엔딩을 검사했습니다.

AdditionalProps-Room.png, AdditionalProps-Desk.png, AdditionalProps-Bathroom.png, AdditionalProps-Appliances.png, AdditionalProps-TrashBin.png, AdditionalProps-Shoes.png는 Unity 카메라 렌더링입니다. 전체 선택 조합의 수동 완주 검사는 별도입니다. 배포본 Windows 빌드 및 실제 실행 확인 결과는 docs/VERIFICATION_KO.md를 확인하세요. 에디터 내부 검색 인덱스의 시작 예외가 발생하면 자동 검사에서 일시정지를 해제하며, 게임 코드의 검사 결과와 구분합니다.

이전 ZIP은 보존했습니다. Backups/BeforeAdditionalProps에 추가 에셋 적용 전 파일을 보관합니다. ZIP에는 Library/Temp 캐시나 사용자 저장 파일을 넣지 않습니다.


## 배치 조정

의자는 이전 방향에서 180도 회전하고 모니터 앞에 정렬했습니다. 컴퓨터 모니터와 화면은 함께 90도 회전해 의자를 바라보도록 옮겼고, 키보드도 앞에 맞췄습니다. 쓰레기통은 사진에서 지정한 신발장과 현관문 사이로 옮겼습니다.

Layout-BathroomDoor-Closed.png / Layout-BathroomDoor-Open.png에서 문 개폐 상태를 확인할 수 있습니다. 최신 검증에는 닫힌 문 양쪽의 충돌, 열린 문 통과, 플레이어 끼임 방지, 냉장고 상호작용 제거와 음식 단독 식사 기능도 포함됩니다.

