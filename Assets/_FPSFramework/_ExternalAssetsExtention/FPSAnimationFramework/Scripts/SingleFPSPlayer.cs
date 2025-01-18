using FPS_Framework;
using UnityEngine;

public class SingleFPSPlayer : MonoBehaviour
{
    [SerializeField]
    protected FPSControllerEx _fpsController;
    public FPSControllerEx FPSController
    {
        get
        {
            return _fpsController;
        }
    }

    [SerializeField]
    protected UI_Ingame _uiHandler;
    public UI_Ingame UiHandler
    {
        get
        {
            return _uiHandler;
        }
    }

    protected void Awake()
    {
        FPSController._SingleFPSPlayer = this;
        UiHandler._SingleFPSPlayer = this;
    }
}
