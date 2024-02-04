using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingTarget : MonoBehaviour
{
    [SerializeField]
    protected GameObject flowTextPrefab;

    [Space(10)]
    [SerializeField]
    protected float flowTime = 1f;
    [SerializeField]
    protected float flowSpeed = 1f;

    [Space(10)]
    [SerializeField]
    protected float maxRandomDistance;

    protected Coroutine postShowTextCoroutine;

    public void ShowText(string title)
    {
        GameObject flowTextInstance = Instantiate(flowTextPrefab);
        Debug.Assert(flowTextInstance.activeSelf == false);

        flowTextInstance.name = title;
        flowTextInstance.transform.position = flowTextPrefab.transform.position;
        flowTextInstance.transform.rotation = flowTextPrefab.transform.rotation;

        flowTextInstance.transform.position = new Vector3(flowTextInstance.transform.position.x + Random.Range(-maxRandomDistance, maxRandomDistance),
                                                          flowTextInstance.transform.position.y + Random.Range(-maxRandomDistance, maxRandomDistance),
                                                          flowTextInstance.transform.position.z + Random.Range(-maxRandomDistance, maxRandomDistance));

        TMP_Text flowText = flowTextInstance.GetComponent<TMP_Text>();
        flowText.text = title;

        flowTextInstance.SetActive(true);

        StartCoroutine(PostShowText(flowTextInstance));
    }

    protected IEnumerator PostShowText(GameObject flowTextInstance)
    {
        float _timer = flowTime;
        while (_timer > 0f)
        {
            yield return null;

            flowTextInstance.transform.Translate(0, _timer * flowSpeed * Time.deltaTime, 0);
            _timer -= Time.deltaTime;
        }

        Destroy(flowTextInstance);
    }
}
