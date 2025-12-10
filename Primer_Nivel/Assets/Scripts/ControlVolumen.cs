using UnityEngine;
using UnityEngine.Audio; // Necesario para el Mixer
using UnityEngine.UI;    // Necesario para los Sliders

public class ControlVolumen : MonoBehaviour
{
    [Header("Referencias")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    // Nombres exactos de los parámetros expuestos en el Mixer
    private const string MIXER_MUSIC = "MusicVol";
    private const string MIXER_SFX = "SFXVol";

    void Start()
    {
        // Cargar valores guardados o usar valor por defecto
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolumePref", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolumePref", 0.75f);

        // Aplicar volumen inicial
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    public void SetMusicVolume(float sliderValue)
    {
        // Convertimos valor lineal (0-1) a Decibelios logarítmicos (-80 a 0)
        mainMixer.SetFloat(MIXER_MUSIC, Mathf.Log10(sliderValue) * 20);

        // Guardamos la preferencia para la próxima vez
        PlayerPrefs.SetFloat("MusicVolumePref", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat(MIXER_SFX, Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SFXVolumePref", sliderValue);
    }
}
