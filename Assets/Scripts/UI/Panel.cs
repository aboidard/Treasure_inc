using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    public GameObject panel;

    private void Start()
    {
        WillStart();
        Close();
    }

    public void Open()
    {
        panel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }
    protected bool IsOpen()
    {
        return panel.activeInHierarchy;
    }

    public void Show()
    {
        if (IsOpen())
        {
            Close();
            return;
        }
        UIManager.instance.CloseAllPanel();
        Open();
        WillShow();
    }

    protected virtual void WillShow()
    {
    }
    protected virtual void WillStart()
    {
    }
}