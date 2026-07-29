using UnityEngine;
using WordVenture.Story;

namespace WordVenture.Tutorial
{
    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;

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
            StoryTelling();
            tutorialCondition = new TutorialConditon002();
        }

        public void OnTriggerTutorial()
        {
            tutorialChatWindow.gameObject.SetActive(true);
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
                tutorialChatWindow.gameObject.SetActive(false);
                return;
            }

            ProceedTutorial();
        }

        public bool IsFlagEqual(TutorialFlag flag)
        {
            return currentFlag.Equals(flag);
        }
    }

}
