using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class UniTaskEx
{
    private static Dictionary<string, CancellationTokenSource> tokenSourceDic = new Dictionary<string, CancellationTokenSource>();

    public static void Cancel(MonoBehaviour monoBehaviour, int index)
    {
        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;

        if (tokenSourceDic.ContainsKey(cancelKey) == true)
        {
            tokenSourceDic[cancelKey].Cancel(false);
            tokenSourceDic[cancelKey].Dispose();
            
            tokenSourceDic.Remove(cancelKey);
        }
    }

    public static void Cancel(string key)
    {
        if (tokenSourceDic.ContainsKey(key) == true)
        {
            tokenSourceDic[key].Cancel(false);
            tokenSourceDic[key].Dispose();

            tokenSourceDic.Remove(key);
        }
    }

    private static void Initialize(MonoBehaviour monoBehaviour, int index)
    {
        if (monoBehaviour == null)
        {
            throw new NullReferenceException("key cannot be null");
        }

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;

        if (tokenSourceDic.ContainsKey(cancelKey) == false)
        {
            tokenSourceDic.Add(cancelKey, new CancellationTokenSource());
        }
    }

    private static void Initialize(string key)
    {
        if (string.IsNullOrEmpty(key) == true)
        {
            throw new NullReferenceException("key cannot be null");
        }

        if (tokenSourceDic.ContainsKey(key) == false)
        {
            tokenSourceDic.Add(key, new CancellationTokenSource());
        }
    }

    // 이 밑은 같은 패턴으로 직접 추가하여 구현해야 합니다. 많이 쓰는것만 일단 해놓음
    public static UniTask NextFrame(MonoBehaviour monoBehaviour, int index)
    {
        Initialize(monoBehaviour, index);

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;
        CancellationToken currentToken = tokenSourceDic[cancelKey].Token;

        return UniTask.NextFrame(currentToken);
    }

    public static UniTask NextFrame(string key)
    {
        Initialize(key);
        CancellationToken currentToken = tokenSourceDic[key].Token;

        return UniTask.NextFrame(currentToken);
    }

    public static UniTask WaitForSeconds(MonoBehaviour monoBehaviour, int index, float duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
    {
        Initialize(monoBehaviour, index);

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;
        CancellationToken currentToken = tokenSourceDic[cancelKey].Token;

        return UniTask.WaitForSeconds(duration, ignoreTimeScale, delayTiming, currentToken);
    }

    public static UniTask WaitForSeconds(string key, float duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
    {
        Initialize(key);
        CancellationToken currentToken = tokenSourceDic[key].Token;

        return UniTask.WaitForSeconds(duration, ignoreTimeScale, delayTiming, currentToken);
    }

    public static UniTask WaitForFixedUpdate(MonoBehaviour monoBehaviour, int index)
    {
        Initialize(monoBehaviour, index);

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;
        CancellationToken currentToken = tokenSourceDic[cancelKey].Token;

        return UniTask.WaitForFixedUpdate(currentToken);
    }

    public static UniTask WaitForFixedUpdate(string key)
    {
        Initialize(key);
        CancellationToken currentToken = tokenSourceDic[key].Token;

        return UniTask.WaitForFixedUpdate(currentToken);
    }

    public static UniTask WaitUntil(MonoBehaviour monoBehaviour, int index, Func<bool> predicate, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
    {
        Initialize(monoBehaviour, index);

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;
        CancellationToken currentToken = tokenSourceDic[cancelKey].Token;

        return UniTask.WaitUntil(predicate, delayTiming, currentToken);
    }

    public static UniTask WaitUntil(string key, Func<bool> predicate, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
    {
        Initialize(key);
        CancellationToken currentToken = tokenSourceDic[key].Token;

        return UniTask.WaitUntil(predicate, delayTiming, currentToken);
    }

    public static UniTask WaitWhile(MonoBehaviour monoBehaviour, int index, Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        Initialize(monoBehaviour, index);

        int id = monoBehaviour.GetInstanceID();
        string cancelKey = id + "_" + monoBehaviour.GetType() + "_" + index;
        CancellationToken currentToken = tokenSourceDic[cancelKey].Token;

        return UniTask.WaitWhile(predicate, timing, currentToken);
    }

    public static UniTask WaitWhile(string key, Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        Initialize(key);
        CancellationToken currentToken = tokenSourceDic[key].Token;

        return UniTask.WaitWhile(predicate, timing, currentToken);
    }
}