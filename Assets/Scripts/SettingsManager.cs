using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager Instance { get; private set; }


    [Header("─── Audio Mixer ───")]
    [Tooltip("Ana AudioMixer asset'i.\n" +
             "Project panelinden sürükle.")]
    [SerializeField]
    private AudioMixer _audioMixer;


    [Header("─── Ses Ayarları UI ───")]
    [Tooltip("Müzik ses seviyesi slider'ı (0–1).")]
    [SerializeField]
    private Slider _musicSlider;    

    [Tooltip("SFX ses seviyesi slider'ı (0–1).")]
    [SerializeField]
    private Slider _sfxSlider;

    [Header("─── Titreşim UI ───")]
    [Tooltip("Titreşim açma/kapama toggle'ı.")]
    [SerializeField]
    private Toggle _vibrationToggle;

    private const string KEY_MUSIC     = "Settings_MusicVol";
    private const string KEY_SFX       = "Settings_SFXVol";
    private const string KEY_VIBRATION = "Settings_Vibration";

    private const string MIXER_MUSIC = "MusicVol";
    private const string MIXER_SFX   = "SFXVol";


    private const float DEFAULT_MUSIC     = 1f;
    private const float DEFAULT_SFX       = 1f;
    private const bool  DEFAULT_VIBRATION = true;


    private bool _vibrationEnabled;


    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadSettings();
        SetupUIListeners();
    }

    private void OnDestroy()
    {
        RemoveUIListeners();

        if (Instance == this)
            Instance = null;
    }

    private void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_MUSIC);
        float sfxVol   = PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_SFX);
        int   vibInt   = PlayerPrefs.GetInt(KEY_VIBRATION, DEFAULT_VIBRATION ? 1 : 0);

        _vibrationEnabled = vibInt == 1;

        ApplyMixerVolume(MIXER_MUSIC, musicVol);
        ApplyMixerVolume(MIXER_SFX, sfxVol);

        if (_musicSlider != null)
        {
            _musicSlider.SetValueWithoutNotify(musicVol);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.SetValueWithoutNotify(sfxVol);
        }

        if (_vibrationToggle != null)
        {
            _vibrationToggle.SetIsOnWithoutNotify(_vibrationEnabled);
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsManager] Ayarlar yüklendi → " +
                  $"Music: {musicVol:F2} | SFX: {sfxVol:F2} | " +
                  $"Vibration: {_vibrationEnabled}");
        #endif
    }

    private void SetupUIListeners()
    {
        if (_musicSlider != null)
            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        if (_vibrationToggle != null)
            _vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
    }

    private void RemoveUIListeners()
    {
        if (_musicSlider != null)
            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);

        if (_vibrationToggle != null)
            _vibrationToggle.onValueChanged.RemoveListener(OnVibrationToggleChanged);
    }

    private void OnMusicSliderChanged(float value)
    {
        SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        SetSFXVolume(value);
    }

    private void OnVibrationToggleChanged(bool isOn)
    {
        ToggleVibration(isOn);
    }

    /// <param name="linearValue">Slider değeri (0.0001 – 1.0)</param>
    public void SetMusicVolume(float linearValue)
    {
        linearValue = Mathf.Clamp(linearValue, 0.0001f, 1f);

        ApplyMixerVolume(MIXER_MUSIC, linearValue);
        PlayerPrefs.SetFloat(KEY_MUSIC, linearValue);
        PlayerPrefs.Save();
    }

    /// <param name="linearValue">Slider değeri (0.0001 – 1.0)</param>
    public void SetSFXVolume(float linearValue)
    {
        linearValue = Mathf.Clamp(linearValue, 0.0001f, 1f);

        ApplyMixerVolume(MIXER_SFX, linearValue);
        PlayerPrefs.SetFloat(KEY_SFX, linearValue);
        PlayerPrefs.Save();
    }

    private void ApplyMixerVolume(string paramName, float linearValue)
    {
        if (_audioMixer == null) return;

        float dB = Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f;
        _audioMixer.SetFloat(paramName, dB);
    }

    public void ToggleVibration(bool isOn)
    {
        _vibrationEnabled = isOn;
        PlayerPrefs.SetInt(KEY_VIBRATION, isOn ? 1 : 0);
        PlayerPrefs.Save();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsManager] Titreşim: {(isOn ? "AÇIK" : "KAPALI")}");
        #endif
    }

    public void TriggerVibrate()
    {
        if (!_vibrationEnabled) return;

        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif
    }

    public void ResetGameData()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[SettingsManager] TÜM VERİ SİLİNİYOR!");
        #endif

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    public bool IsVibrationEnabled => _vibrationEnabled;
}
