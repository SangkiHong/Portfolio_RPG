using System.Collections;
using UnityEngine;

namespace SK.Practice
{
    public class CoroutineExample : MonoBehaviour
    {

        int coroutineCount;
        int updateCount;

        float coroutineElapsed;
        float updateElapsed;

        IEnumerator Start()
        {
            coroutineCount = 0;
            updateCount = 0;

            coroutineElapsed = 0f;
            updateElapsed = 0f;

            //StartCoroutine(test());
            //StartCoroutine(WaitUnilFunction());

            yield return StartCoroutine(DisplayData_1());
            yield return StartCoroutine(DisplayData_2());
            yield return StartCoroutine(DisplayData_3());
            yield return StartCoroutine(DisplayData_4());
        }

        IEnumerator WaitUnilFunction()
        {
            Debug.Log("Count가 20이 안되었습니다.");
            yield return new WaitUntil(() => updateCount >= 20);
            Debug.Log("Count가 20이상이 되었습니다");
            yield break;
        }

        IEnumerator test()
        {
            while (true)
            {
                coroutineElapsed += Time.deltaTime;
                if (coroutineElapsed <= 1f)
                    coroutineCount++;
                else
                {
                    Debug.Log($"coroutineCount: {coroutineCount}");
                    coroutineElapsed -= 1f;
                    coroutineCount = 0;
                }
                yield return null;
            }
        }

        IEnumerator DisplayData_1()
        {
            Debug.Log("1");
            yield return null;
        }

        IEnumerator DisplayData_2()
        {
            Debug.Log("2");
            var count = 0;
            while (count <= 10)
            {
                count++;
                Debug.Log($"Count: {count}");
            }
            yield return null;
        }

        IEnumerator DisplayData_3()
        {
            Debug.Log("3");
            yield return null;
        }

        IEnumerator DisplayData_4()
        {
            Debug.Log("4");
            yield return null;
        }

        private void Update()
        {
            updateElapsed += Time.deltaTime;
            if (updateElapsed <= 1f)
                updateCount++;
            else
            {
                Debug.Log($"updateCount: {updateCount}");
                updateElapsed -= 1f;
                updateCount = 0;
            }
        }
    }
}