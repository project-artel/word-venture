using UnityEngine;

namespace Scenes.Test
{
    public sealed class KeyboardTest : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space key was pressed.");
            }
            if (Input.GetKeyUp(KeyCode.Return))
            {
                Debug.Log("Return key was released.");
            }
        }
    }
}
