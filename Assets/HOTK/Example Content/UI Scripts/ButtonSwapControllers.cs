using UnityEngine;
using System.Collections;

public class ButtonSwapControllers : MonoBehaviour
{
    public void OnButtonClicked()
    {
        HOTK_TrackedDeviceManager.Instance.SwapControllers();
    }
}
