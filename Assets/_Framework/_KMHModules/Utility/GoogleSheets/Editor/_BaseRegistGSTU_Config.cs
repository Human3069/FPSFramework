using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// _GoogleSheetsToUnityConfig 위치를 저장하기 위한 ScriptableObject <para/>
/// CreateAssetMenu는 필요할때 사용
/// </summary>
#if true
    [CreateAssetMenu(fileName = "_BaseRegistGSTU_Config", menuName = "ScriptableObjects/BaseRegistGSTU_Config", order = int.MaxValue)]
#endif
public class _BaseRegistGSTU_Config : ScriptableObject
{
    public _BaseGoogleSheetsToUnityConfig GstuConfig;
}
