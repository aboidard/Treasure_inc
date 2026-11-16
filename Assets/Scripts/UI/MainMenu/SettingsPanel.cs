using UnityEngine;
using UnityEngine.UI;
public class SettingsPanel : Panel
{
    public Text idUtilisateur;
    public static SettingsPanel instance;
    private Text soundLabel;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    protected override void WillStart()
    {
        // the scene has no setting for the sounds: the button is added between the title and the user id
        Button button = UIBuilder.Button(UIBuilder.Child("SoundButton", panel.transform, new Vector2(0.25f, 0.67f), new Vector2(0.75f, 0.8f)),
            "", 46, ToggleSound);
        soundLabel = button.GetComponentInChildren<Text>();
        UpdateSoundLabel();
    }

    private void ToggleSound()
    {
        SoundManager.Instance.Muted = !SoundManager.Instance.Muted;
        UpdateSoundLabel();
        // heard at once when the sounds come back
        SoundManager.Play(Sfx.Coins);
    }

    private void UpdateSoundLabel()
    {
        soundLabel.text = SoundManager.Instance.Muted ? "Sons : coupés" : "Sons : activés";
    }

    protected override void WillShow()
    {
        idUtilisateur.text = NetworkManager.Instance.publicKey;

        UIManager.instance.setCameraPosition(CameraMovement.POSITION_SETTINGS);
    }

    public void OnClickSite()
    {
        Application.OpenURL("https://www.treasure-inc.fr/chest?id=" + NetworkManager.Instance.publicKey);
    }
}
