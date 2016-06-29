using UnityEngine;

public class HOTK_DemoDesktopCameraScript : MonoBehaviour
{
    public float Size = 200f;
    public void LateUpdate()
    {
        Camera.main.orthographicSize = Screen.height / Size;
    }
}
