# Linux 빌드에 ARTEL TC 검증 추가

`.github/workflows/artel-qa.yml`은 설정 확인 → Linux Development Build → ARTEL TC
검증 순서로 실행합니다. 같은 저장소의 PR과 `develop` push, 수동 실행을 지원합니다.
Fork PR은 secret을 전달하지 않고 실행을 건너뜁니다.

## GitHub 설정 등록

저장소 **Settings → Secrets and variables → Actions**에서 등록합니다.

| 종류 | 이름 | 값 |
| --- | --- | --- |
| Secret | `ARTEL_TOKEN` | 해당 ARTEL project에 접근 가능한 CLI token |
| Variable | `ARTEL_PROJECT_ID` | WordVenture의 ARTEL project ID |
| Variable | `ARTEL_TEST_RUN_ID` | 해당 project에 속하는 기존 test-run ID |
| Secret | `UNITY_EMAIL` | GameCI에서 사용할 Unity 계정 |
| Secret | `UNITY_PASSWORD` | Unity 계정 비밀번호 |
| Secret | `UNITY_LICENSE` | GameCI에 사용할 license 파일 내용 |
| Secret | `UNITY_SERIAL` | serial 방식 사용 시 등록. `UNITY_LICENSE`와 둘 중 하나 필요 |

현재 선택한 대상은 project `11`(WordVenture 사용자 관점 테스트)의 test-run
`28`(WordVenture 코드 기반 사용자 관점 테스트)입니다. 저장 데이터가 없는 상태를 전제하며,
각 실행에서 새 Linux runner를 사용합니다.

Unity 인증은 [GameCI activation 안내](https://game.ci/docs/github/activation/)를 따릅니다.
workflow가 license를 발급하지는 않습니다. 설정이 없으면 `설정 확인` job이 누락된 이름을
표시하고 실패하며, Unity 빌드와 QA는 시작하지 않습니다.

## Linux 빌드 구성

Unity 2022.3.62f3으로 Build Settings의 enabled scene을 빌드합니다.
`ArtelCiBuild.BuildLinux`는 SDK의 QA 기능을 포함하도록 `BuildOptions.Development`를
사용합니다. QA 빌드에만 OpenGLCore를 지정하고 기존 graphics 설정은 빌드 후 복원합니다.
실행 파일뿐 아니라 `_Data`, shared library를 포함한 전체 player를 `artel-linux-player`
artifact에 저장한 뒤 Linux runner로 전달합니다.

QA job은 Ubuntu 24.04에서 Xvfb의 1280×720 가상 화면과 Mesa software rendering을 사용합니다.
artifact 다운로드 후 실행 권한을 복원하고, display와 OpenGL 초기화를 확인한 뒤 게임을 실행합니다.
`artel-linux-rendering` artifact에는 Xvfb 및 OpenGL 진단 로그가 저장됩니다.
Unity/WebRTC가 `libX11.so`를 직접 로드하므로 runtime library와 함께 `libx11-dev`도 설치합니다.

ARTEL SDK는 `Packages/manifest.json`의 Git commit으로 고정했습니다. 개인 PC의 외부
폴더가 없어도 같은 package를 받을 수 있습니다. 기존 WebGL/release workflow는 수정하지
않았지만, 공통 SDK dependency가 바뀌므로 해당 대상도 다음 빌드에서 compilation 확인이 필요합니다.

## TC 실행 결과 확인

`qa-action`은 모델 선택을 지원하는 commit SHA로 고정했습니다. `model` 입력은
`claude/claude-sonnet-5-5`이며, ARTEL 서버에 등록된 공용 Claude API key로 호출합니다.
GitHub runner에 모델 API key를 추가할 필요는 없습니다. 기본 ARTEL stage 환경에
연결하며, 게임 등록은 120초, QA 판정은 1800초까지 기다립니다. 완료된 `PASSED`만 CI 성공이며
실패·판정 불명·시간 초과는 실패입니다. Job summary와 `artel-qa-*` artifact에서 상세 결과와
진단 로그를 확인합니다.

처음에는 짧은 시나리오가 포함된 test-run으로 확인하세요. 테스트 데이터와 새 설치 상태가
일치해야 합니다. GitHub runner에서의 Unity 렌더링과 SDK screen capture는 별도로 검증해야
합니다. 일반 Linux runner에서 그래픽 초기화가 실패하면 게임을 지원하는 일회용 GPU runner가
필요할 수 있습니다. 화면 검증이므로 player에 `-batchmode`나 `-nographics`를 추가하지 않습니다.

## 변경 검증

workflow 구문, dependency 고정과 artifact 연결을 검사했습니다. Unity 인증 설정이
완료된 뒤 실제 게임 빌드 및 TC 결과를 확인해야 합니다.
draft PR을 검토하고 `develop`에 반영하면 Actions의 수동 실행으로도 검증할 수 있습니다.
