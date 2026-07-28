using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace WordVenture.Story
{
    public class StoryController : MonoBehaviour
    {

        public ChatWindowController chatWindowController;
        [SerializeField] ChatWindowScriptContainer scriptContainer;

        [SerializeField] List<GameObject> backgorunds = new List<GameObject>();

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip badMood;

        void Start()
        {
            InitBackground();
            StartCoroutine(StoryTelling());

        }

        private void InitBackground()
        {

            backgorunds[0].SetActive(true);
            for (int i = 1; i < backgorunds.Count; i++)
            {
                backgorunds[i].SetActive(false);
            }
        }

        private void SwitchBackground(int id)
        {
            for (int i = 0; i < backgorunds.Count; i++)
            {
                if (i == id)
                {
                    backgorunds[i].SetActive(true);
                }
                else
                {
                    backgorunds[i].SetActive(false);
                }

            }
            if (id == 1)
            {
                audioSource.clip = badMood;
                audioSource.Play();
            }
        }


        IEnumerator StoryTelling()
        {
            for (int i = 0; i < scriptContainer.GetScriptNum(); i++)
            {
                SwitchBackground(scriptContainer.GetScriptData(i).background);
                chatWindowController.UpdateChatStream(scriptContainer.GetScriptData(i).name, scriptContainer.GetScriptData(i).text);

                // 타이핑 연출이 끝날 때까지 기다리되, Space를 누르면 즉시 끝낸다.
                // 재생 시간을 따로 계산해 기다리면 실제 코루틴보다 항상 조금 짧게 끝나서
                // 이전 대사의 스트리밍이 살아 있는 채로 다음 대사가 시작된다.
                yield return new WaitUntil(() => !chatWindowController.IsStreaming || Input.GetKeyDown(KeyCode.Space));
                if (chatWindowController.IsStreaming)
                {
                    chatWindowController.CompleteStream();
                    yield return null;
                }

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }

            LoadMapScene();
        }

        private void LoadMapScene()
        {
            if (WordVenture.Map.MapMove.StagePosition == 5)
                SceneManager.LoadScene("TitleScene");
            else
                SceneManager.LoadScene("Map_scene");
        }
    }
}
