using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework
{
    /// <summary>
    /// A class for migration from version 1.x to 2.x
    /// </summary>
    public class MigrationHelperEx
    {
        private Object migratedObject;
        
        public void StartMigrate(ScratchCardHandlerEx cardHandler)
        {
            if (cardHandler == null)
            {
                return;
            }

            bool result = false;

            if (cardHandler.MeshRendererCard == null)
            {
                FieldInfo field = cardHandler.GetType().GetField("meshRendererCard");
                object meshCardValue = field.GetValue(cardHandler);
                if (meshCardValue != null)
                {
                    GameObject meshCardObj = (GameObject)meshCardValue;
                    if (meshCardObj != null)
                    {
                        if (meshCardObj.TryGetComponent<MeshRenderer>(out var meshRenderer))
                        {
                            cardHandler.MeshRendererCard = meshRenderer;
                            field.SetValue(cardHandler, null);
                            result = true;
                        }
                    }
                }
            }
            
            if (cardHandler.SpriteRendererCard == null)
            {
                FieldInfo field = cardHandler.GetType().GetField("SpriteCard");
                object spriteCardValue = field.GetValue(cardHandler);
                if (spriteCardValue != null)
                {
                    GameObject spriteCardObj = (GameObject)spriteCardValue;
                    if (spriteCardObj != null)
                    {
                        if (spriteCardObj.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                        {
                            cardHandler.SpriteRendererCard = spriteRenderer;
                            field.SetValue(cardHandler, null);
                            result = true;
                        }
                    }
                }
            }

            if (cardHandler.RenderType == ScratchCardAsset.Core.ScratchCardRenderType.CanvasRenderer)
            {
                if (cardHandler.CanvasRendererCard == null)
                {
                    FieldInfo field = cardHandler.GetType().GetField("canvasRendererCard");
                    object imageCardValue = field.GetValue(cardHandler);
                    if (imageCardValue != null)
                    {
                        GameObject imageCardObj = (GameObject)imageCardValue;
                        if (imageCardObj != null)
                        {
                            if (imageCardObj.TryGetComponent<Image>(out Image image) == true)
                            {
                                cardHandler.CanvasRendererCard = image;
                                field.SetValue(cardHandler, null);
                                result = true;
                            }
                        }
                    }
                }
            }

            if (result == true)
            {
                migratedObject = cardHandler;
                Debug.Log($"The migration for {cardHandler} was successful!", cardHandler);
            }
        }

        public void FinishMigrate()
        {
            if (migratedObject != null)
            {
                MarkAsDirty(migratedObject);
                migratedObject = null;
            }
        }

        private void MarkAsDirty(Object unityObject)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(unityObject);
                var component = unityObject as Component;
                if (component != null)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
            }
#endif
        }
    }
}