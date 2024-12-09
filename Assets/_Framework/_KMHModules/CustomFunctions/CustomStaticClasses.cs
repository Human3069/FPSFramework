using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _KMH_Framework
{
    public static class Vector3Ex
    {
        #region Get Float Value...
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 betweenLength = b - a;
            Vector3 currentLength = value - a;

            return Vector3.Dot(currentLength, betweenLength) / Vector3.Dot(betweenLength, betweenLength);
        }

        public static float GetAngleFromThreePositions(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            Vector3 L1 = pos1 - pos2;
            Vector3 L2 = pos3 - pos2;

            float dotProducts = (L1.normalized.x * L2.normalized.x) + (L1.normalized.y * L2.normalized.y) + (L1.normalized.z * L2.normalized.z);
            double angle = (Mathf.Acos(dotProducts) * 180.0) / Mathf.PI;
            float result = Mathf.Round((float)angle * 1000) / 1000;

            return result;
        }

        public static float GetDistanceTwoLine(Vector3 aStartPoint, Vector3 aEndPoint, Vector3 bStartPoint, Vector3 bEndPoint)
        {
            Vector3 _aLength = aStartPoint - aEndPoint;
            Vector3 _bLength = bStartPoint - bEndPoint;
            Vector3 _startToStartLength = aStartPoint - bStartPoint;

            Vector3 line = Vector3.Cross(_aLength, _bLength);

            return Mathf.Abs(Vector3.Dot(_startToStartLength, line)) / line.magnitude;
        }
        #endregion

        #region Get One Point...
        public static Vector3 RotateToAngle(Vector3 originDir, float angle)
        {
            return Vector3.RotateTowards(originDir, -originDir, angle * Mathf.PI / 180f, 0f);
        }

        public static Vector3 GetClosestPointOnFiniteLine(Vector3 targetPoint, Vector3 startPoint, Vector3 endPoint)
        {
            Vector3 _lineDirection = endPoint - startPoint;

            float _lineLength = _lineDirection.magnitude;
            _lineDirection.Normalize();

            float _dot = Vector3.Dot(targetPoint - startPoint, _lineDirection);
            float _projectLength = Mathf.Clamp(_dot, 0f, _lineLength);
            return startPoint + _lineDirection * _projectLength;
        }

        public static Vector3 GetPredictPosition(Vector3 myPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
        {
            float _magnitude = (targetPos - myPos).magnitude;
            Vector3 targetPredict = targetPos + (targetVelocity * _magnitude / projectileSpeed);

            return targetPredict;
        }

        public static Vector3 GetPredictPositionOnGravity(Vector3 myPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
        {
            float _magnitude = (targetPos - myPos).magnitude;
            float floatTime = _magnitude / projectileSpeed;
            float gravityApproximate = (float)((2.3f * floatTime * floatTime) + (2.4 * floatTime));

            Vector3 targetPredict = targetPos + (targetVelocity * floatTime);
            targetPredict -= new Vector3(0f, gravityApproximate, 0f);

            return targetPredict;
        }
        #endregion

        #region Get Two Points...
        public static (Vector3 pointA, Vector3 pointB) GetShortestTwoPoint(Vector3 aStartPoint, Vector3 aEndPoint, Vector3 bStartPoint, Vector3 bEndPoint, bool isUnclamped = false)
        {
            Vector3 _aLength = aStartPoint - aEndPoint;
            Vector3 _bLength = bStartPoint - bEndPoint;

            // get right angle between two Line
            Vector3 _rightAngleOfAB = Vector3.Cross(_aLength, _bLength);

            Vector3 _crossedLineA = Vector3.Cross(_rightAngleOfAB, _aLength);
            Vector3 _crossedLineB = Vector3.Cross(_rightAngleOfAB, _bLength);

            Vector3 _pointADir = (aEndPoint - aStartPoint).normalized;
            Vector3 _pointBDir = (bEndPoint - bStartPoint).normalized;

            // Find intersection points
            Vector3 _pointA = aStartPoint + _pointADir * Vector3.Dot(_crossedLineB, bStartPoint - aStartPoint) / Vector3.Dot(_crossedLineB, _pointADir);
            Vector3 _pointB = bStartPoint + _pointBDir * Vector3.Dot(_crossedLineA, aStartPoint - bStartPoint) / Vector3.Dot(_crossedLineA, _pointBDir);

            if (isUnclamped == false)
            {
                // Clamp points over endPoints
                _pointA = Vector3.ClampMagnitude(_pointA - aStartPoint, _aLength.magnitude) + aStartPoint;
                _pointB = Vector3.ClampMagnitude(_pointB - bStartPoint, _bLength.magnitude) + bStartPoint;

                // Clamp points over startPoints
                _pointA = Vector3.ClampMagnitude(_pointA - aEndPoint, _aLength.magnitude) + aEndPoint;
                _pointB = Vector3.ClampMagnitude(_pointB - bEndPoint, _bLength.magnitude) + bEndPoint;
            }

            return (_pointA, _pointB);
        }

        public static (Vector3 pointA, Vector3 pointB) GetShortestTwoPoint(Ray rayA, Ray rayB)
        {
            Vector3 aStartPoint = rayA.origin;
            Vector3 aEndPoint = rayA.origin + rayA.direction;

            Vector3 bStartPoint = rayB.origin;
            Vector3 bEndPoint = rayB.origin + rayB.direction;

            return GetShortestTwoPoint(aStartPoint, aEndPoint, bStartPoint, bEndPoint, true);
        }
        #endregion
    }

    public static class ExtentionMethods
    {
        public static void Swap<T>(this List<T> list, int from, int to)
        {
            T tmp = list[from];
            list[from] = list[to];
            list[to] = tmp;
        }

        public static bool TryFind(this Transform _t, string name, out Transform _foundT)
        {
            _foundT = _t.Find(name);
            return _foundT != null;
        }

        public static bool Similiar(this float origin, float comparer, float limit)
        {
            float difference = Mathf.Abs(origin - comparer);
            float absLimit = Mathf.Abs(limit);

            return absLimit > difference;
        }

        public static string ToEncryptAES(this string originalText, string key)
        {
            RijndaelManaged rijndaelCipher = GetRijndaelCipher(key);
            byte[] textBytes = Encoding.UTF8.GetBytes(originalText);

            string encrypted = Convert.ToBase64String(rijndaelCipher.CreateEncryptor().TransformFinalBlock(textBytes, 0, textBytes.Length));
            return encrypted;
        }

        public static string ToDecryptAES(this string targetText, string key)
        {
            RijndaelManaged rijndaelCipher = GetRijndaelCipher(key);
            byte[] encryptedData = Convert.FromBase64String(targetText);
            byte[] decryptedAsBytes = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(decryptedAsBytes);
        }

        private static RijndaelManaged GetRijndaelCipher(string key)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];

            int _length = passwordBytes.Length;
            if (_length > keyBytes.Length)
            {
                _length = keyBytes.Length;
            }
            Array.Copy(passwordBytes, keyBytes, _length);

            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }
    }

    // UI 기본적인 요구사항 중 Tab 누르면 다음 Selectable로 전환되는 요구가 많습니다.
    // 그걸 구현하려고 만든것입니다.
    public class SelectionCircularList 
    {
        // 기본적으로 Unity C# 에서는 순환 LinkedList를 지원하지 않음. 따라서 별도로 지정해야 합니다.
        private LinkedList<Selectable> selectableLinkedList = new LinkedList<Selectable>();

        public SelectionCircularList(params Selectable[] selectables)
        {
            Debug.Assert(selectables.Length > 1);

            for (int i = 0; i < selectables.Length; i++)
            {
                selectableLinkedList.AddLast(selectables[i]);
            }

            SelectFirst();
        }

        public void SelectFirst()
        {
            if (selectableLinkedList.Count > 0)
            {
                selectableLinkedList.First.Value.Select();
            }
        }

        public Selectable GetCurrent()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return null;
            }
            else
            {
                GameObject selectedObj = currentEventSystem.currentSelectedGameObject;
                if (selectedObj != null)
                {
                    if (selectedObj.TryGetComponent<Selectable>(out Selectable foundSelectable) == true)
                    {
                        LinkedListNode<Selectable> node = selectableLinkedList.Find(foundSelectable);
                        return node.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public bool TryMoveNext()
        {
            Selectable currentSelectable = GetCurrent();
            if (currentSelectable != null)
            {
                LinkedListNode<Selectable> node = selectableLinkedList.Find(currentSelectable);
                bool isMovable = (node != null);
                if (isMovable == true)
                {
                    if (node.Next == null)
                    {
                        selectableLinkedList.First.Value.Select();
                    }
                    else
                    {
                        node.Next.Value.Select();
                    }
                }

                return isMovable;
            }
            else
            {
                return false;
            }
        }
    }
}