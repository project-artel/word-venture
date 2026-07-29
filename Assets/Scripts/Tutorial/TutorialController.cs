using System.Collections;
using UnityEngine;
using WordVenture.Core;
using WordVenture.Story;

namespace WordVenture.Tutorial
{
    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;

        // 대화창 뒤의 UI(카드, Back 버튼 등) 클릭을 막는 전체 화면 이미지.
        [SerializeField] GameObject inputBlocker;

        [SerializeField] TutorialFlag currentFlag = TutorialFlag.FLAG_001_START_TUTORIAL;
        [SerializeField] ITutorialCondition tutorialCondition;

        // 대사를 다 읽었다는 확인 입력을 기다리는 중인지. 확인 입력을 받기 전에는
        // 다음 대사로 넘어가지 않는다.
        bool waitingForAcknowledge;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            if (WordVenture.Map.MapMove.StagePosition > 0)
            {
                gameObject.SetActive(false);
                return;
            }
            SetChatWindowVisible(true);
            StoryTelling();
            tutorialCondition = new TutorialConditon002();
        }

        public void OnTriggerTutorial()
        {
            SetChatWindowVisible(true);
            GoNextFlag();
            StoryTelling();
            tutorialCondition = tutorialCondition.GetNextCondition();
        }

        void GoNextFlag()
        {
            currentFlag = currentFlag.Next();
        }

        void StoryTelling()
        {
            TutorialChatData tutorialChatData = tutorialScript.GetScriptData(currentFlag);
            tutorialChatWindow.SetSpeakerImage(tutorialScript.GetSprite(tutorialChatData.portraitID));
            tutorialChatWindow.SetAnyKeyPromptVisible(false);
            tutorialChatWindow.UpdateChatStream(tutorialChatData.name, tutorialChatData.text);
            waitingForAcknowledge = true;
        }

        public void ProceedTutorial()
        {
            if(tutorialCondition.IsMeetCondition())
            {
                OnTriggerTutorial();
            }
        }

        private void Update()
        {
            if(currentFlag.Equals(TutorialFlag.FLAG_014_END_TUTORIAL))
            {
                gameObject.SetActive(false);
                return;
            }

            if (waitingForAcknowledge)
            {
                if (!IsAdvanceKeyDown())
                {
                    return;
                }

                // 타이핑 중이면 첫 입력은 연출 스킵으로 쓴다. 연타로 대사가 통째로 날아가지 않게 한다.
                if (tutorialChatWindow.IsStreaming)
                {
                    tutorialChatWindow.CompleteStream();
                    return;
                }

                waitingForAcknowledge = false;
                tutorialChatWindow.SetAnyKeyPromptVisible(false);
                SetChatWindowVisible(false);
                return;
            }

            ProceedTutorial();
        }

        /// <summary>
        /// 대화창과 입력 차단을 함께 켜고 끈다. 창만 켜면 뒤의 카드·버튼이 그대로 눌린다.
        /// </summary>
        void SetChatWindowVisible(bool visible)
        {
            tutorialChatWindow.gameObject.SetActive(visible);

            if (inputBlocker != null)
            {
                inputBlocker.SetActive(visible);
            }

            if (visible)
            {
                InteractionLock.IsLocked = true;
                return;
            }

            if (!isActiveAndEnabled)
            {
                InteractionLock.IsLocked = false;
                return;
            }

            StartCoroutine(UnlockAfterFrame());
        }

        /// <summary>
        /// 확인 입력이 발생한 프레임에는 잠금을 유지한다. 같은 프레임에 Map의 Enter 처리 같은
        /// 다른 Update가 이어서 돌면, 대사를 넘긴 키가 게임플레이 입력으로도 먹힌다.
        /// </summary>
        IEnumerator UnlockAfterFrame()
        {
            yield return null;
            InteractionLock.IsLocked = false;
        }

        // 튜토리얼이 끝나거나 오브젝트가 꺼지면 코루틴이 죽으므로 잠금을 직접 푼다.
        private void OnDisable()
        {
            InteractionLock.IsLocked = false;
        }

        public bool IsFlagEqual(TutorialFlag flag)
        {
            return currentFlag.Equals(flag);
        }
    }

}
