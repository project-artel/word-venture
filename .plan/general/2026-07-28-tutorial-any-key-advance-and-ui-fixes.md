# 2026-07-28 — 2차 스프린트: 게임 튜토리얼 수정

- Date: 2026-07-28
- Jira: ARTEL-155
- Status: Ready

## Goal

1. 튜토리얼 대사가 시간 경과가 아니라 **아무 키 입력**으로 넘어가도록 변경한다.
2. "아무 키나 눌러 진행" 안내를 플레이어에게 노출한다.
3. Map 화면의 Back 버튼이 화면 밖으로 잘리는 문제를 고친다.
4. 스토리 대사를 연타로 넘길 때 텍스트가 깜박이는 문제를 고친다. (이슈 본문 외 추가 요청)

## Non-goals

- `ITutorialCondition` 체인의 게임플레이 조건 자체는 바꾸지 않는다.
- 튜토리얼 스크립트 데이터(`TutorialScript.asset`) 문구는 바꾸지 않는다.
- Back 버튼의 크기·스프라이트·동작(`BackButton.BackToMain`)은 바꾸지 않는다. 앵커/위치만 고친다.
- 신규 Input System 도입 없음. 기존 레거시 `Input` API 유지.
- 스토리 씬(StoryScene/EndingScene)의 진행 키는 Space를 유지한다. 이슈 본문이 튜토리얼만 명시한다.

## Context / Constraints

### 현재 동작

- [TutorialController.cs:75](Assets/Scripts/Tutorial/TutorialController.cs:75) — `Update()`가
  `Time.time > chatRemainTime`(타이핑 종료 후 3초)이면 `ProceedTutorial()`을 호출한다. 즉
  **플레이어 입력 없이** 자동으로 넘어간다.
- [TutorialController.cs:79](Assets/Scripts/Tutorial/TutorialController.cs:79) — `chatCloseTime`(5초) 뒤
  대화창을 자동으로 닫는다. 조건이 게임플레이로 충족되는 구간(예: 카드 조합)에서 창을 비켜주는 역할이다.
- `ProceedTutorial()`은 `tutorialCondition.isMeetCondition()`이 참일 때만 다음 플래그로 넘어간다.
  타이머는 "조건 체크 트리거"일 뿐이므로, 트리거를 키 입력으로 바꿔도 조건 체인은 그대로 동작한다.

### 깜박임 원인

`ChatWindowController.UpdateChatStream()`이 매번 `StartCoroutine`을 **핸들 없이** 띄운다
([ChatWindowController.cs:21](Assets/Scripts/Story/ChatWindowController.cs:21)).
`UpdateStreamingChat`은 `text.Length + 1`회의 `WaitForSeconds(0.03f)`를 도는데 각 yield가 프레임 경계까지
올림되어 60fps에서 실제 소요는 약 `N * 0.0333s`다. 반면 `StoryController`가 기다리는 시간은 `N * 0.03f`로
약 11% 짧다. 그래서 대기가 먼저 끝나고, Space를 연타하면 **이전 대사의 스트리밍 코루틴이 살아 있는 채로**
다음 대사의 코루틴이 추가로 시작된다. 두 코루틴이 같은 `chatText`에 서로 다른 substring을 프레임마다
번갈아 `SetText` 하면서 깜박인다. 대사가 길수록 누적 오차가 커져 재현이 쉽다.

### 클래스 구조상의 제약

`TutorialChatWindow : ChatWindowController`이지만 상속을 쓰지 않고 **복제**하고 있다.
`nameText`/`descriptText`는 base의 `chatName`/`chatText`와 같은 것을 가리키는 별도 필드이고,
`InitSetting()`은 base `InitTmp_text()`와 동일한 로직이며, `UpdateChatStream`은 `new`로 base를 가린다.
이렇게 된 이유는 두 클래스가 모두 `private void Awake()`를 선언해서 Unity가 파생 클래스의 것만 호출하기
때문이다(base의 `InitTmp_text()`가 실행되지 않아 base 필드가 null이다).

따라서 코루틴 소유권을 base에 한 번만 구현하려면 `Awake`/초기화 경로를 먼저 정리해야 한다.
이건 DRY 취향 문제가 아니라 **한 번의 수정으로 두 화면의 깜박임을 함께 고치기 위한 전제 조건**이다.

### Back 버튼 위치

[Map_scene.unity](Assets/Scenes/Map_scene.unity)의 `Button (Legacy)`(RectTransform fileID 1006407981):
`m_AnchorMin/Max = (0.5, 0.5)`, `m_AnchoredPosition = (450, -440)`, `m_SizeDelta = (400, 400)`.
CanvasScaler는 `ScaleWithScreenSize`, 레퍼런스 1920x1080, `m_MatchWidthOrHeight = 0`(가로 기준).
캔버스 하단은 y = -540인데 버튼 하단은 y = -640 → **100px이 화면 밖으로 잘린다**.
중앙 앵커이므로 16:9보다 넓은 화면비에서는 잘림이 더 심해진다.

### 테스트 환경

`Assets` 아래에 `.asmdef`도 테스트 어셈블리도 없다. 자동화 테스트를 새로 세우는 것은 이 이슈의 범위 밖이다.
검증은 **배치모드 컴파일 + 에디터 수동 재생**으로 한다. Unity 2022.3.34f1이 설치되어 있다.

## Approach (Checklist)

- [x] **Step 0: Recon** — `Assets/Scripts/Tutorial/`, `Assets/Scripts/Story/`,
      `Assets/Scripts/Cards/BackButton.cs`, `Assets/Scenes/Map_scene.unity`,
      `Assets/Prefabs/Tutorial/TutorialController.prefab` 확인 완료.

- [ ] **Step 1: `ChatWindowController`가 스트리밍을 단독 소유**
  - `Awake()`를 base에만 두고, 텍스트 탐색을 `protected virtual void InitTexts()`로 뺀다.
  - 탐색을 **이름 기반**으로 바꾼다: `GetComponentsInChildren<TMP_Text>(true)`를 돌며
    `ChatName` / `ChatText`를 이름으로 찾는다. 인덱스 0/1 가정은 자식 텍스트가 하나만 늘어도 깨진다.
    Step 3에서 안내용 TMP_Text를 추가하므로 이 변경이 선행되어야 한다.
    (StoryScene, EndingScene, TutorialController.prefab 모두 이 두 이름을 쓰는 것을 확인함.)
  - 실행 중인 스트리밍 코루틴을 필드로 들고, `UpdateChatStream()` 진입 시 `StopCoroutine`으로 정리한 뒤
    새로 시작한다. → **깜박임의 근본 원인 제거**.
  - `public bool IsStreaming { get; }`와 `public void CompleteStream()`(전체 텍스트 즉시 표시)을 공개한다.
  - 스트리밍 종료 시 `protected virtual void OnStreamComplete()`를 호출한다.

- [ ] **Step 2: `TutorialChatWindow` 중복 제거**
  - `nameText`/`descriptText`/`InitSetting()`/`private void Awake()`/`new UpdateChatStream`/
    `UpdateStreamingChat`/`ChatStatus` enum/`chatCloseTime`/`chatRemainTime`/
    `Constant.CHAT_CLOSE_TIME`/`Constant.CHAT_REMAIN_TIME`/`Constant.TUTORIAL_TEXT_TIME`을 제거한다.
    (`ChatStatus`와 두 시간 필드는 `TutorialController` 외에 참조처가 없음을 grep으로 확인함.)
  - 남는 것은 `speakerImage` + `SetSpeakerImage()` + 안내 표시 제어뿐이다.

- [ ] **Step 3: 튜토리얼 진행을 키 입력으로 전환**
  - `TutorialController`에 `bool waitingForAcknowledge` 상태를 둔다.
  - 진행 키 판정: `Input.anyKeyDown`에서 마우스 버튼을 뺀다.
    전투 중 카드 클릭이 대사를 넘겨버리면 안 되기 때문이다.
    ```csharp
    static bool IsAdvanceKeyDown() =>
        Input.anyKeyDown
        && !Input.GetMouseButtonDown(0)
        && !Input.GetMouseButtonDown(1)
        && !Input.GetMouseButtonDown(2);
    ```
  - `Update()` 상태 기계:
    1. 스트리밍 중 + 진행 키 → `CompleteStream()` (타이핑 스킵). 연타로 대사가 통째로 날아가지 않는다.
    2. 스트리밍 끝 + `waitingForAcknowledge` → 안내 표시. 진행 키를 받으면
       `waitingForAcknowledge = false`, 안내 숨김, 대화창 숨김.
       (기존 `chatCloseTime` 자동 닫힘이 하던 "창을 비켜주는" 역할을 명시적 확인 입력이 대신한다.)
    3. `waitingForAcknowledge == false` + `isMeetCondition()` → `OnTriggerTutorial()`로 다음 대사.
  - `OnTriggerTutorial()`은 대사 출력 후 `waitingForAcknowledge = true`로 둔다.
  - `Time.time` 기반 분기 두 개는 삭제한다. `FLAG_014_END_TUTORIAL` 종료 처리는 유지한다.

- [ ] **Step 4: "아무 키" 안내 UI**
  - `TutorialChatWindow`에 `[SerializeField] GameObject anyKeyPrompt`와
    `SetAnyKeyPromptVisible(bool)`을 추가한다. 참조가 비어 있어도 죽지 않게 null 가드를 둔다.
  - `TutorialController.prefab`의 `ChatWindow` 아래에 `AnyKeyPrompt` TMP_Text를 추가하고
    (문구: `아무 키나 누르세요`) 참조를 연결한다. 기존 `ChatText` 블록을 복제해 fileID/이름/위치/문구만 바꾼다.
  - 스트리밍 중에는 숨기고, 스트리밍이 끝나 확인 입력을 기다리는 동안에만 켠다.

- [ ] **Step 5: Map Back 버튼 위치 수정**
  - `Map_scene.unity`의 RectTransform 1006407981을 우측 하단 앵커로 변경:
    `m_AnchorMin/Max = (1, 0)`, `m_Pivot = (1, 0)`, `m_AnchoredPosition = (-40, 40)`.
  - 크기 400x400 유지. 어떤 화면비에서도 잘리지 않고 여백 40px이 보장된다.

- [ ] **Step 6: 검증** — Validation 절 참고.

- [ ] **Step 7: Rollout / Rollback** — 피처 플래그 없음. 샘플 게임 한정이라 `git revert`로 되돌린다.

## Validation

- **Commands to run:**
  ```bash
  /Applications/Unity/Hub/Editor/2022.3.34f1/Unity.app/Contents/MacOS/Unity \
    -batchmode -quit -nographics -projectPath . -logFile - | tail -40
  ```
  컴파일 에러가 없어야 한다. 이후 에디터에서 수동 재생.
- **Expected output:**
  1. StoryScene: 대사 진행 중 Space 연타 → 깜박임 없음. 첫 입력은 타이핑 즉시 완료,
     두 번째 입력에서 다음 대사로 이동.
  2. TurnBattleScene 튜토리얼: 대사가 3초 뒤 자동으로 넘어가지 않는다. 키를 눌러야 진행한다.
  3. 스트리밍이 끝나면 `아무 키나 누르세요` 안내가 뜨고, 키를 누르면 안내와 대화창이 사라진다.
  4. 카드 조합 등 게임플레이 조건 구간: 확인 입력 후 창이 닫히고, 조건 충족 시 창이 다시 열리며 다음 대사.
  5. 전투 중 카드 클릭(마우스)으로는 대사가 넘어가지 않는다.
  6. Map_scene: 16:9 / 21:9 / 4:3 모두에서 Back 버튼 전체가 보이고, 클릭 시 TitleScene으로 이동.

## Risks & Rollback

- **Risks:**
  - `Input.anyKeyDown`은 마우스를 포함한다. 제외를 빠뜨리면 전투 클릭이 대사를 넘긴다 → Step 3에서 명시 처리.
    같은 프레임에 키와 마우스를 동시에 누르면 진행 입력이 무시된다. 실사용에서 문제되지 않는 수준으로 본다.
  - 자동 진행 제거로 진행이 완전히 플레이어 입력에 걸린다. 조건 미충족 구간에서 창이 닫힌 뒤에는
    안내를 띄우지 않으므로, 플레이어가 "멈춘 것"으로 오해할 여지가 있다. 이 구간의 대사 자체가
    행동을 지시하므로 허용 가능한 트레이드오프로 본다.
  - `Awake` 초기화 경로를 base로 옮기면 `TutorialChatWindow`의 초기화 시점이 바뀐다.
    `speakerImage`는 인스펙터 참조라 영향 없지만, 컴파일 후 실제 재생으로 확인해야 한다.
  - 씬/프리팹 YAML을 에디터 없이 직접 편집한다. fileID 충돌·값 오타 위험이 있으므로
    편집 후 Unity에서 열어 정상 로드되는지 확인한다.
- **Rollback steps:** `git revert <commit>` — 외부 상태 변경 없음.

## Rejected feedback

- **`ChatStatus` enum을 남겨두고 `IsStreaming`과 병행** — 참조처가 `TutorialController` 한 곳뿐이라
  상태 표현을 둘로 유지할 이유가 없다. 제거한다.
- **스토리 씬도 "아무 키"로 통일** — 이슈 본문이 튜토리얼만 명시한다. 범위를 넓히지 않는다.
- **`Constant` 클래스 전체 삭제** — `TUTORIAL_TEXT_TIME`이 base의 타이핑 간격과 같은 값이라 함께
  정리하지만, `Constant` 클래스 자체는 다른 용도로 남을 수 있으므로 빈 껍데기 정리만 한다.

## Open Questions

- 회의록(Notion `3ab0bce5-474c-803a-aed3-c4305f62c996`)을 읽지 못했다. 환경에 `NOTION_API_TOKEN`이
  없어 `ntn`이 `No workspace selected`로 실패한다. **Back 버튼의 목표 위치가 회의에서 특정됐다면
  우측 하단이 아닐 수 있다.** 현재는 "잘리지 않게 화면 안으로" 를 요구사항으로 가정하고 진행한다.
- 깜박임 수정은 이슈 본문에 없는 추가 요청이다. ARTEL-155에 포함할지 별도 이슈로 뺄지 확인 필요.
