# 책상 종이·약봉투 적용 — 0.2.1

- `papers.zip`의 `SM_PapersCluster`를 책상 앞쪽 왼편 정리 대상으로 사용했습니다. 기존 컵 오브젝트를 대체하며 F키 정리 기능과 정리 후 숨김을 유지합니다.
- `Don't Draw_Medicine Bag.zip`의 SketchUp 모델을 가져와 약봉투와 3개 복용 포장을 Unity 메시·프리팹으로 정리했습니다. 봉투 인쇄와 아침·점심·저녁 포장 텍스처를 복원했고, 기존 약 블록을 대체합니다.
- 약봉투는 책상 앞쪽 오른편에 배치해 날짜별 기존 종이 블록과 겹치지 않도록 했습니다. 복약 선택 시 숨김, 복약을 미루면 유지, 다음 날과 저장 불러오기 시 표시 복원을 유지합니다.
- 최신 master의 대사·목표·정신력·저장·날짜 진행·오디오 코드는 바꾸지 않았습니다.

## 편집 위치

- 프리팹: `Assets/ThirdParty/AdditionalProps/GamePrefabs/DeskPapers.prefab`, `MedicineBag.prefab`
- 배치·상호작용: `Assets/Scripts/Environment/AdditionalRoomPropsPalette.cs`
- 참조 설정: `Assets/Settings/AdditionalRoomPropsPalette.asset`의 deskPapers, medicineBag
- 약봉투 원본·추출 텍스처: `Assets/ThirdParty/DeskMedicine`
- 종이 변환 입력·텍스처: `Assets/ThirdParty/AdditionalProps/PreparedInput/papers`

기존 동작 코드와 연결하기 위해 런타임 이름 `Cup - Click to Tidy`, `Medicine - Interact`를 유지합니다. 새 모델로 치환한 원래 블록은 Legacy Placeholder 이름으로 비활성화했습니다.

## 확인 결과

Unity Play 모드에서 이번 소품 관련 **33개 검사 PASS**. F키 조사용 Collider·타깃, 재질·인쇄 텍스처, 정리·복약·미루기 선택, 다음 날 복원과 저장 불러오기를 검사했습니다. 결과: `DeskPropsValidation.txt`.

Windows 0.2.1 빌드 성공, 오류 0·경고 0. 결과: `DeskPropsWindowsBuild.txt`. 이번 실행 파일은 `Builds/DeskPropsWindows/DExpressive.exe`에 있습니다. 이전 0.2.0의 전체 진행 검사와 이번 소품 검사는 별도 기록입니다.

적용 화면: `DeskProps-Desk.png`, `DeskProps-Papers.png`, `DeskProps-Medicine.png`. Unity의 실제 본편 카메라로 렌더링했습니다. 원본 약봉투의 구매자 사용·재배포 제한 안내는 DeskMedicine/README-original.txt에 보관합니다.
