# D-EXPRESSIVE — v0.2.2 제출 자료

방 안에서 작은 행동과 선택으로 열흘을 보내는 1인칭 심리 서사 게임입니다. 최종 master 코드의 날짜별 목표, 정신력, 저장·불러오기, 체크포인트·회귀, 엔딩, 오디오와 기상 연출을 보존하고 실내 에셋을 적용했습니다.

## 실행 및 프로젝트

- [최신 Windows 실행 파일과 에셋 ZIP](https://github.com/kkk1038/D-Expressive/releases/tag/v0.2.2)
- Windows ZIP 전체를 압축 해제한 뒤 `DExpressive.exe`를 실행합니다. Data 폴더와 DLL을 함께 유지하세요.
- Unity 프로젝트: [DExpressive](DExpressive). Unity **6000.3.22f1**, URP **17.3.0**으로 열고 `Assets/Scenes/SampleScene.unity`를 실행합니다.
- 조작: WASD 이동, 마우스 시점, F 상호작용. 화면에 표시되는 선택과 조작 안내를 따르세요.

## 이번 버전

- 최종 master의 Day 1~10 시스템에 실내·가구·욕실·생활 소품을 통합했습니다.
- 책상 정리 대상을 종이 더미, 약 복용 대상을 약봉투 모델로 교체했습니다. 선택 결과와 날짜 변경·저장 복원에 따라 표시 상태가 갱신됩니다.
- 욕조를 180도 반전한 방향으로 유지하고 약 2m 길이로 확대해 욕실 공간에 맞췄습니다.
- 현관 옆 쓰레기통, 의자·모니터 방향, 음식 전용 식사 기능, 열고 닫는 욕실 문을 유지합니다.
- 클로드 전용 프롬프트는 이번 제출 파일에서 제외했습니다. 과거 릴리스와 Git 이력은 유지합니다.

## 문서

- [최신 제출·검증 안내](DExpressive/docs/SUBMISSION_V022_KO.md)
- [에셋 위치와 사용 방법](DExpressive/docs/ASSET_GUIDE_KO.md)
- [에셋 목록·해시](DExpressive/docs/ASSET_INVENTORY.csv)
- [최종 코드 통합 기록](DExpressive/MASTER_ASSET_INTEGRATION_KO.md)
- [종이·약봉투 교체 기록](DExpressive/DESK_PROPS_UPDATE_KO.md)
- [외부 에셋 라이선스·출처](DExpressive/THIRD_PARTY_NOTICES.md)

외부 에셋의 개별 이용 조건이 적용됩니다. 저장소는 기존 비공개 상태를 유지하며 프로젝트 전체에 별도 오픈소스 라이선스를 부여하지 않습니다.
