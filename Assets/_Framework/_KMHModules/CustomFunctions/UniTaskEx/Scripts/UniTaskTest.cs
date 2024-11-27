using Cysharp.Threading.Tasks;
using UnityEngine;

public class UniTaskTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestAsync_key_0().Forget();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UniTaskEx.Cancel(this, 0);
        }

        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestAsync_key_1().Forget();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UniTaskEx.Cancel(this, 1);
        }
    }

    private async UniTask TestAsync_key_0()
    {
        float time = 0f;
        while (true)
        {
            Debug.Log("T : " + time);

            await UniTaskEx.NextFrame(this, 0);
            time += Time.deltaTime;
        }
    }

    private async UniTask TestAsync_key_1()
    {
        float time = 0f;
        while (true)
        {
            Debug.Log("T : " + time);

            await UniTaskEx.NextFrame(this, 1);
            time += Time.deltaTime;
        }
    }
}