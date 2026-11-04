# GitHub 업로드 안내

## 들어 있는 것

통합 배포 ZIP에는 Unity 소스, 게임 설명 문서, Windows 게임이 함께 들어 있습니다. 루트 `.gitignore`가 `Builds`, `Library`, `Temp`, `Logs`, `UserSettings`, IDE 생성물, 저장 파일을 제외합니다. `.meta` 파일과 `Packages/packages-lock.json`은 반드시 유지합니다.

## 소스 업로드

1. 통합 ZIP을 풀고 `README.md`, `Assets`, `Packages`, `ProjectSettings`가 보이는 DExpressive 폴더를 선택합니다.
2. GitHub Desktop에서 해당 폴더로 새 로컬 저장소를 만듭니다. 기존 README나 .gitignore를 덮어쓰지 않습니다.
3. 변경 파일을 커밋하고 **Publish repository**에서 우선 **Private**로 게시합니다.
4. 실행 파일은 아래 Releases 절차로 따로 첨부합니다.

터미널을 쓰는 경우:

```powershell
git init -b main
git add .
git commit -m "Prepare D-EXPRESSIVE playable prototype v0.1.0"
git remote add origin https://github.com/YOUR_ACCOUNT/YOUR_REPOSITORY.git
git push -u origin main
```

Git 사용자 이름·이메일과 GitHub 로그인은 본인 계정으로 설정합니다. 이 압축본에는 계정 정보나 인증 토큰이 들어 있지 않습니다. GitHub 원격 저장소 생성·게시 자체는 자동 수행하지 않았습니다.

## 게임 배포

GitHub 저장소의 **Releases → Draft a new release**에서 태그 `v0.1.0`을 만들고 `DExpressive-Windows-x64.zip`을 첨부합니다. `docs/RELEASE_NOTES_KO.md` 내용을 설명에 사용합니다. 실행용 ZIP 전체를 첨부하고 EXE만 올리지 마세요.

대용량 실행 파일·통합 배포 ZIP은 Git 커밋 대신 Releases에 첨부합니다. 원본 소스의 가장 큰 파일도 GitHub의 100 MiB 제한 미만인지 패키징 시 검사합니다. 일부 모델은 웹 업로드 제한인 25 MiB를 넘으므로 소스 업로드에는 GitHub Desktop이나 Git을 사용합니다.

공식 기준: [GitHub 파일 크기와 바이너리 배포 안내](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github).

## 공개 여부

외부 3D 모델의 제공 ZIP에 제작자·배포 페이지·라이선스 문서가 모두 포함되어 있지는 않았습니다. `THIRD_PARTY_NOTICES.md`에 상태를 적었습니다. 소스를 공개하면 원본 모델도 공개되므로, 재배포 허가를 확인한 후 Public으로 전환하세요. 실행 파일의 공개 배포도 게임에 포함하는 조건을 확인한 뒤 진행합니다. 임의로 MIT 등 전체 라이선스를 붙이지 않습니다.
