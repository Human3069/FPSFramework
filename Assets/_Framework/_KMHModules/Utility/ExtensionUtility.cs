using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _KMH_Framework._TS_Module
{
    static public partial class ExtensionUtility
    {
        /// <summary>
        /// Color
        /// </summary>
        public static Color ToColor(this string strColor)
        {
            if (ColorUtility.TryParseHtmlString(strColor, out var color) == false)
            {
                return Color.white;
            }
            return color;
        }
        public static string SetColorOfText(this string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static string ToHtmlString(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static bool IsValueDigit(this string value)
        {
            return Regex.IsMatch(value, @"^[-+]?\d*\.?\d+$");
        }
        public static bool ValueDigit(this string text, out int value)
        {
            return int.TryParse(Regex.Replace(text, "[^0-9]", ""), out value);
        }
        /// <summary>
        /// Editor(Window, Mac): "프로젝트디렉토리/Assets" <para/>
        /// Android: "jar:file:///data/app/번들이름.apk!/assets" <para/>
        /// iOS: "/var/mobile/Applications/프로그램ID/앱이름.app/Data"
        /// </summary>
        public static string GetAssetPath()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Application.streamingAssetsPath;
#elif UNITY_IOS && !UNITY_EDITOR
        return Application.streamingAssetsPath
#else
            return Application.dataPath;
#endif
        }


        static T[] RemoveIndices<T>(this T[] array, int[] indices)
        {
            return array.Where((item, index) => !indices.Contains(index)).ToArray();
        }

        /// <summary>
        /// Log
        /// </summary>
        public static void Log(this string logMesseage, LogType logType = LogType.Log, StackTraceLogType traceLogType = StackTraceLogType.ScriptOnly)
        {
            Application.SetStackTraceLogType(logType, traceLogType);
            if (logType == LogType.Log)
            {
                Debug.Log(logMesseage);
            }
            else if (logType == LogType.Warning)
            {
                Debug.LogWarning(logMesseage);
            }
            else
            {
                Debug.LogError(logMesseage);
            }
        }

        /// <summary>
        /// Vector
        /// </summary>
        public static Vector2 ToVector2(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }
        public static void Inverse(Vector3 pos, Quaternion rot, out Vector3 p, out Quaternion r)
        {
            var m = Matrix4x4.TRS(pos, rot, Vector3.one);
            var invM = m.inverse;

            p = invM.MultiplyPoint(Vector3.zero);
            r = invM.rotation;
        }

        /// <summary>
        /// Transform
        /// </summary>
        public static Transform FindObjectByObjectName(this Transform transform, string objectName)
        {
            foreach (Transform child in transform)
            {
                var trn = child.Find(objectName);
                if (trn != null)
                {
                    return trn;
                }
                child.FindObjectByObjectName(objectName);
            }
            return null;
        }
        public static void ResetTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        public static void SetLayer(this Transform transform, string layerName, bool recursive = false)
        {
            var layer = LayerMask.NameToLayer(layerName);
            transform.gameObject.layer = layer;

            if (recursive)
            {
                SetChildLayersHelper(transform, layer, recursive);
            }
        }
        private static void SetChildLayersHelper(Transform transform, int layer, bool recursive)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layer;

                if (recursive)
                {
                    SetChildLayersHelper(child, layer, true);
                }
            }
        }

        // UnityEvent를 UnityAction으로 사용하기 위한 래퍼 메서드
        public static UnityAction<T> GetUnityActionFromEvent<T>(this UnityEvent<T> unityEvent)
        {
            return (T) => unityEvent.Invoke(T);
        }
        public static UnityAction<T1, T2> GetUnityActionFromEvent<T1, T2>(this UnityEvent<T1, T2> unityEvent)
        {
            return (T1, T2) => unityEvent.Invoke(T1, T2);
        }
        public static UnityAction<T1, T2, T3> GetUnityActionFromEvent<T1, T2, T3>(this UnityEvent<T1, T2, T3> unityEvent)
        {
            return (T1, T2, T3) => unityEvent.Invoke(T1, T2, T3);
        }

        /// <summary>
        /// UI
        /// </summary>
        public static void UIRefresh(this RectTransform layoutRoot)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }
#if false
    /// <summary>
    /// UniTask <para/>
    /// time : MilliSecond
    /// </summary>
    public static async UniTask AsyncDelayTime(int time, Action call)
    {
        await UniTask.Delay(time);
        call?.Invoke();
    }
    public static async UniTask AsyncOperationHandle(AsyncOperationHandle op, Action call)
    {
        await op;
        call?.Invoke();
    }
#endif
        /// <summary>
        /// Coroutine
        /// </summary>
        public static IEnumerator CoDelayCall(float time, Action call)
        {
            yield return new WaitForSeconds(time);
            call?.Invoke();
        }
#if false
    public static IEnumerator CoAsyncOperationHandle(AsyncOperationHandle op, Action call)
    {
        yield return op;
        call?.Invoke();
    }
#endif
        /// <summary>
        /// RenderTexture
        /// </summary>
        public static byte[] RenderTextureToJPG(int width, int height, int quality = 50)
        {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            return tex.EncodeToJPG(quality);
        }
        public static byte[] RenderTextureToPNG(int width, int height)
        {
            var texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            return texture2D.EncodeToPNG();
        }
        public static Texture2D ResizeTextureToBytes(Texture2D texture, int targetWidth, int targetHeight, int maxSize = 1920)
        {
            if (targetWidth > maxSize)
            {
                targetHeight = Mathf.FloorToInt((float)targetHeight * maxSize / targetWidth);
                targetWidth = maxSize;
            }
            else
            {
                targetWidth = Mathf.FloorToInt((float)targetWidth * maxSize / targetHeight);
                targetHeight = maxSize;
            }

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

            Color[] pixels = result.GetPixels(0);

            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);

            for (int pixel = 0; pixel < pixels.Length; pixel++)
            {
                pixels[pixel] = texture.GetPixelBilinear(incX * ((float)pixel % targetWidth), incY * ((float)Mathf.Floor(pixel / targetWidth)));
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
        public static List<Material> AddMaterialList(this Renderer renderer, params Material[] data)
        {
            var materials = renderer.materials.ToList();
            foreach (var material in data)
            {
                materials.Add(material);
            }
            return materials;
        }
        /// <summary>
        /// Clipboard
        /// </summary>
        public static void SetClipboard(this string str) => GUIUtility.systemCopyBuffer = str;
        public static string GetClipboard() => GUIUtility.systemCopyBuffer;
    }
}