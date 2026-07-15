using System;
using System.Collections;
using Artel.Tracking;
using UnityEngine;

public sealed class TrackingTest : MonoBehaviour
{
    [ArtelState("int")] private int trackingInt;

    [ArtelState("string")] private string trackingString;

    [ArtelAction("success_void")]
    private void SuccessVoid()
    {
    }

    [ArtelAction("success_int")]
    private int SuccessInt()
    {
        return 1;
    }

    [ArtelAction("fail_void")]
    private void FailVoid()
    {
        throw new Exception("의도된 실패");
    }

    private void Start()
    {
        StartCoroutine(CallActions());
    }

    private IEnumerator CallActions()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            trackingInt++;
            trackingString = "string-" + trackingInt;
            SuccessVoid();
            SuccessInt();
            try
            {
                FailVoid();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
