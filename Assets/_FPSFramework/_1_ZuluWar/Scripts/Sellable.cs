using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sellable : MonoBehaviour
{
    [SerializeField]
    protected GameObject uiPrefab;

    [Space(10)]
    [SerializeField]
    protected float price = 2000;
}
