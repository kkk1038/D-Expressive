# 실행·개발·빌드 안내

## Windows 게임 실행

1. 통합 ZIP 또는 Windows 실행용 ZIP을 원하는 폴더에 **전체 압축 해제**합니다.
2. 통합 ZIP은 `Builds/Windows/DExpressive.exe`, 실행 전용 ZIP은 `DExpressive.exe`를 실행합니다.
3. 새 게임 시작을 누르고, 첫 대화에서 선택지를 고릅니다.
4. WASD와 마우스로 이동하고 F로 물건을 조사합니다. 침대로 다음 날을 진행합니다.
5. Esc로 일시정지하거나 Alt+F4로 종료합니다.

EXE만 따로 옮기면 실행되지 않습니다. 같은 폴더에 포함된 DLL·데이터·런타임 폴더를 함께 배포하세요. Unity Editor나 Python은 플레이에 필요하지 않습니다. Windows x64용으로 빌드했으며 macOS·모바일 실행 파일은 포함하지 않았습니다.

## 저장 파일

`%USERPROFILE%/AppData/LocalLow/DefaultCompany/DExpressive/dexpressive_save.json`

Unity의 `Application.persistentDataPath`를 사용합니다. 배포 파일에는 개인 저장 데이터가 없습니다. 같은 PC에서 에디터나 이전 버전으로 만든 저장이 있다면 불러오기 버튼이 이를 사용할 수 있습니다. F5로 저장하면 기존 한 슬롯을 덮어씁니다.

실행 로그 역시 위 폴더의 `Player.log`를 확인합니다. 실행용 폴더가 쓰기 금지여도 저장은 사용자 데이터 폴더에 기록됩니다.

## Unity 프로젝트 열기

- Unity Hub에 Unity **6000.3.22f1**과 Windows Build Support를 설치합니다.
- `Assets`, `Packages`, `ProjectSettings`가 있는 DExpressive 폴더를 프로젝트로 추가합니다.
- 최초 실행 시 인터넷으로 Packages 의존성을 복원합니다. Library는 자동 재생성됩니다.
- `Assets/Scenes/SampleScene.unity`를 열고 Play를 누릅니다.
- `Assets/Scenes/InteriorKitTest.unity`는 모델 확인용 별도 씬이며 본편이 아닙니다.

방의 대부분은 실행 시 생성됩니다. Play 중 Hierarchy의 생성 물건 위치를 바꾸는 것만으로는 영구 저장되지 않습니다. 지속적인 배치 변경은 `InteriorKitGamePalette.cs`, `AdditionalRoomPropsPalette.cs` 또는 관련 Palette 에셋에 반영하세요. 이전 자동 생성 코드를 별도로 추가하면 중복될 수 있습니다.

## Windows 재빌드

Unity 메뉴 **DExpressive → Build Windows Release**를 선택합니다. `Assets/Editor/ReleaseBuild.cs`가 다음 설정으로 빌드합니다.

| 항목 | 값 |
| --- | --- |
| 대상 | Windows x64 |
| 시작 씬 | Assets/Scenes/SampleScene.unity |
| 백엔드 | Mono |
| 모드 | 일반 배포 빌드 |
| 초기 화면 | 1280×720 창 모드, 크기 변경 가능 |
| 버전 | 0.1.0 |
| 결과 | Builds/Windows/DExpressive.exe |

명령줄 빌드 예시(PowerShell, 경로는 PC에 맞게 수정):

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.22f1/Editor/Unity.exe' `
  -batchmode -quit -projectPath $PWD.Path `
  -executeMethod ReleaseBuild.Windows -logFile ./build.log
```

동일 프로젝트가 이미 Unity에서 열려 있으면 메뉴 빌드를 사용하거나 에디터를 닫은 뒤 명령줄 빌드를 사용합니다. 결과 요약은 `Builds/build-result.txt`입니다.

## 회귀 검사 다시 실행

새로 열린 Unity 프로세스에서 `-executeMethod InteriorKitGameVerification.Begin`을 사용하면 Play 모드 자동 검사를 수행하고 종료합니다. `-quit`은 붙이지 않습니다. 기존 코드 비교를 위해 `Backups/PrototypeRoomBootstrap.original.cs.txt`를 유지하세요. 결과는 `InteriorKitGameValidation.txt`입니다. 로컬 저장을 백업·복원하지만, 실제 게임을 동시에 실행하면서 검사하지 않는 것이 좋습니다.
