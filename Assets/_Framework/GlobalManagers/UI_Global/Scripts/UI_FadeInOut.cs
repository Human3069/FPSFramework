using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeInOut : MonoBehaviour
{
    private const string LOG_FORMAT = "<color=white><b>[UI_FadeInOut]</b></color> {0}";

    protected static UI_FadeInOut _instance;
    public static UI_FadeInOut Instance
    {
        get
        {
            return _instance;
        }
        protected set
        {
            _instance = value;
        }
    }

    [SerializeField]
    protected Image image;
    [SerializeField]
    protected TextMeshProUGUI fadeOverText;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogErrorFormat(LOG_FORMAT, "");
            Destroy(this.gameObject);
            return;
        }

        Debug.Assert(image != null);

        Color _color = image.color;
        _color.a = 0f;
        image.color = _color;

        fadeOverText.color = new Color(fadeOverText.color.r, fadeOverText.color.g, fadeOverText.color.b, 0f);
    }

    protected virtual void OnDestroy()
    {
        UniTaskEx.Cancel(this, 0);

        if (Instance != this)
        {
            return;
        }

        Instance = null;
    }

    public virtual async UniTask FadeInAsync(float duration)
    {
        Color targetColor = new Color(image.color.r, image.color.g, image.color.b, 0f);
        await FadeInAsync(duration, targetColor);
    }

    public virtual async UniTask FadeInAsync(float duration, Color color)
    {
        UniTaskEx.Cancel(this, 0);

        Color originColor = image.color;
        Color targetColor = new Color(color.r, color.g, color.b, 0f);
        image.color = new Color(color.r, color.g, color.b, 1f);

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            image.color = Color.Lerp(originColor, targetColor, normalized);

            await UniTaskEx.NextFrame(this, 0);
            timer += Time.unscaledDeltaTime;
            normalized = timer / duration;
        }
        image.color = targetColor;
    }

    public virtual async UniTask FadeOutAsync(float duration, Color color)
    {
        UniTaskEx.Cancel(this, 0);

        image.color = new Color(color.r, color.g, color.b, 0f);
        Color originColor = image.color;

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            image.color = Color.Lerp(originColor, color, normalized);

            await UniTaskEx.NextFrame(this, 0);
            timer += Time.unscaledDeltaTime;
            normalized = timer / duration;
        }
        image.color = color;
    }

    public virtual async UniTask ShowText(string text, float duration)
    {
        fadeOverText.text = text;
        Color originColor = fadeOverText.color;
        Color targetColor = new Color(originColor.r, originColor.g, originColor.b, 1f);

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            fadeOverText.color = Color.Lerp(originColor, targetColor, normalized);

            await UniTaskEx.NextFrame(this, 0);
            timer += Time.unscaledDeltaTime;
            normalized = timer / duration;
        }
        fadeOverText.color = targetColor;
    }

    public virtual async UniTask HideText(float duration)
    {
        Color originColor = fadeOverText.color;
        Color targetColor = new Color(originColor.r, originColor.g, originColor.b, 0f);

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            fadeOverText.color = Color.Lerp(originColor, targetColor, normalized);

            await UniTaskEx.NextFrame(this, 0);
            timer += Time.unscaledDeltaTime;
            normalized = timer / duration;
        }
        fadeOverText.color = targetColor;
    }

    public virtual async UniTask FloatText(string text, float duration)
    {
        await FadeOutAsync(duration, Color.black);

        await ShowText(text, duration);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            await UniTask.Yield();
        }

        await HideText(duration);

        await FadeInAsync(duration, Color.black);
    }
}
