using System;
using UnityEngine.UI;
using UnityEngine;
using WordVenture.Story;

namespace WordVenture.Tutorial
{
    public static class Extensions
    {

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }

    public enum TutorialFlag
    {
        NONE = 0,
        FLAG_001_START_TUTORIAL = 1,
        FLAG_002_BATTLE_START = 2,
        FLAG_003_TURN_START = 3,
        FLAG_004_COMBINATION = 4,
        FLAG_005_COMBINATION_DESCRIPT = 5,
        FLAG_006_SET_MAGIC = 6,
        FLAG_007_SET_ELEMENTAL = 7,
        FLAG_008_CAST_SPELL = 8,
        FLAG_009_CAST_END = 9,
        FLAG_010_CLICK_TO_SELECT = 10,
        FLAG_011_FINISH_SPELL = 11,
        FLAG_012_NEXT_ENEMY = 12,
        FLAG_013_END_BATTLE = 13,
        FLAG_014_END_TUTORIAL = 14,
        END = 15,
    }

    public class TutorialChatWindow : ChatWindowController
    {
        [SerializeField] Image speakerImage;
        [SerializeField] GameObject anyKeyPrompt;

        public void SetSpeakerImage(Sprite image)
        {
            speakerImage.sprite = image;
        }

        /// <summary>
        /// "아무 키나 누르세요" 안내를 켜고 끈다. 프리팹에 안내 오브젝트가
        /// 연결되지 않아도 튜토리얼 자체는 동작해야 한다.
        /// </summary>
        public void SetAnyKeyPromptVisible(bool visible)
        {
            if (anyKeyPrompt == null)
            {
                return;
            }

            anyKeyPrompt.SetActive(visible);
        }

        protected override void OnStreamComplete()
        {
            SetAnyKeyPromptVisible(true);
        }
    }

}
