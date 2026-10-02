using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlarmModule : MonoBehaviour
{
    [Header("UI Alarma")]
    [SerializeField] private TMP_Dropdown dropdownTonos;
    [SerializeField] private TextMeshProUGUI txtHeaderAlarma;

    [Header("Botones Físicos de Control")]
    [SerializeField] private Button btnHH;          // Botón 1 (HH)
    [SerializeField] private Button btnMM;          // Botón 2 (MM)
    [SerializeField] private Button btnPlayPreview; // Botón 3 (Play preview)
    [SerializeField] private Button btnSave;        // Botón 4 (SAVE)

    [Header("Botones Emergentes (Alarma Sonando)")]
    [SerializeField] private GameObject panelBotonesDisparo;
    [SerializeField] private Button btnDetenerAlarma;
    [SerializeField] private Button btnPausarAlarma;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> listaTonos = new List<AudioClip>();

    [Header("Dependencias")]
    [SerializeField] private AppModeManager modeManager;

    private const string PREF_HORA = "Alarm_Hora";
    private const string PREF_MINUTO = "Alarm_Minuto";
    private const string PREF_TONO = "Alarm_TonoIndex";
    private const string PREF_ACTIVA = "Alarm_Activa";

    // Configuración base/manual de la alarma
    private int horaAlarma = 13;
    private int minutoAlarma = 0;

    // Variables de postergación (Snooze)
    private bool esAlarmaPostergeda = false;
    private int horaPostergeda;
    private int minutoPostergeda;

    private bool alarmaActiva = false;
    private bool sonandoActualmente = false;

    private void Awake()
    {
        if (btnHH != null) btnHH.onClick.AddListener(OnHHPressed);
        if (btnMM != null) btnMM.onClick.AddListener(OnMMPressed);
        if (btnPlayPreview != null) btnPlayPreview.onClick.AddListener(OnPlayPreviewPressed);
        if (btnSave != null) btnSave.onClick.AddListener(OnSavePressed);

        if (btnDetenerAlarma != null) btnDetenerAlarma.onClick.AddListener(DetenerAlarmaCompletamente);
        if (btnPausarAlarma != null) btnPausarAlarma.onClick.AddListener(PostergarAlarma5Minutos);

        if (dropdownTonos != null)
        {
            dropdownTonos.onValueChanged.AddListener(OnTonoCambiado);
        }
    }

    private void Start()
    {
        CargarConfiguracion();
        OcultarBotonesDisparo();
    }

    private void Update()
    {
        ComprobarDisparoAlarma();
    }

    private void OnHHPressed()
    {
        if (modeManager == null || modeManager.ObtenerModoActual() != AppModeManager.AppMode.Alarma) return;

        horaAlarma = (horaAlarma + 1) % 24;
        SincronizarVariableAdicional();
        ActualizarPantallaAlarma();
    }

    private void OnMMPressed()
    {
        if (modeManager == null || modeManager.ObtenerModoActual() != AppModeManager.AppMode.Alarma) return;

        minutoAlarma = (minutoAlarma + 10) % 60;
        SincronizarVariableAdicional();
        ActualizarPantallaAlarma();
    }

    private void SincronizarVariableAdicional()
    {
        if (!esAlarmaPostergeda)
        {
            horaPostergeda = horaAlarma;
            minutoPostergeda = minutoAlarma;
        }
    }

    private void OnTonoCambiado(int index)
    {
        DetenerAudio();
        CargarClipPorIndice(index);
    }

    private void OnPlayPreviewPressed()
    {
        if (modeManager == null || modeManager.ObtenerModoActual() != AppModeManager.AppMode.Alarma) return;

        if (audioSource != null && audioSource.clip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else
            {
                audioSource.Play();
            }
        }
    }

    private void OnSavePressed()
    {
        if (modeManager == null || modeManager.ObtenerModoActual() != AppModeManager.AppMode.Alarma) return;

        alarmaActiva = true;
        sonandoActualmente = false;
        esAlarmaPostergeda = false;

        SincronizarVariableAdicional();
        GuardarConfiguracion();
        ActualizarPantallaAlarma();
        ActualizarHeaderUI();

        DetenerAudio();
        OcultarBotonesDisparo();
    }

    private void CargarClipPorIndice(int index)
    {
        if (audioSource == null || listaTonos.Count == 0) return;

        if (index >= 0 && index < listaTonos.Count && listaTonos[index] != null)
        {
            audioSource.clip = listaTonos[index];
        }
    }

    private void GuardarConfiguracion()
    {
        PlayerPrefs.SetInt(PREF_HORA, horaAlarma);
        PlayerPrefs.SetInt(PREF_MINUTO, minutoAlarma);

        if (dropdownTonos != null)
        {
            PlayerPrefs.SetInt(PREF_TONO, dropdownTonos.value);
        }

        PlayerPrefs.SetInt(PREF_ACTIVA, alarmaActiva ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CargarConfiguracion()
    {
        horaAlarma = PlayerPrefs.GetInt(PREF_HORA, 13);
        minutoAlarma = PlayerPrefs.GetInt(PREF_MINUTO, 0);
        alarmaActiva = PlayerPrefs.GetInt(PREF_ACTIVA, 0) == 1;
        esAlarmaPostergeda = false;

        SincronizarVariableAdicional();

        int tonoGuardado = PlayerPrefs.GetInt(PREF_TONO, 0);
        if (dropdownTonos != null && tonoGuardado < dropdownTonos.options.Count)
        {
            dropdownTonos.value = tonoGuardado;
            dropdownTonos.RefreshShownValue();
        }

        CargarClipPorIndice(tonoGuardado);
        ActualizarPantallaAlarma();
        ActualizarHeaderUI();
    }

    private void ActualizarHeaderUI()
    {
        if (txtHeaderAlarma != null)
        {
            string estado = $"{horaAlarma:00}:{minutoAlarma:00}";
            txtHeaderAlarma.text = $"ALARMA: {estado} H";
        }
    }

    private void ActualizarPantallaAlarma()
    {
        int h = esAlarmaPostergeda ? horaPostergeda : horaAlarma;
        int m = esAlarmaPostergeda ? minutoPostergeda : minutoAlarma;

        string horaFormateada = $"{h:00}:{m:00}:00";
        if (modeManager != null)
        {
            modeManager.ActualizarHoraAlarmaDisplay(horaFormateada);
        }
    }

    private void ComprobarDisparoAlarma()
    {
        if (!alarmaActiva || sonandoActualmente) return;

        string horaActual = DateTime.Now.ToString("HH:mm");

        int targetH = esAlarmaPostergeda ? horaPostergeda : horaAlarma;
        int targetM = esAlarmaPostergeda ? minutoPostergeda : minutoAlarma;
        string horaObjetivo = $"{targetH:00}:{targetM:00}";

        if (horaActual == horaObjetivo)
        {
            DispararAlarma();
        }
    }

    private void DispararAlarma()
    {
        sonandoActualmente = true;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
        MostrarBotonesDisparo();
    }

    public void DetenerAlarmaCompletamente()
    {
        sonandoActualmente = false;
        alarmaActiva = false;
        esAlarmaPostergeda = false; // Vuelve a false

        SincronizarVariableAdicional(); // Copia el valor guardado
        GuardarConfiguracion();
        ActualizarHeaderUI();

        DetenerAudio();
        OcultarBotonesDisparo();
    }

    public void PostergarAlarma5Minutos()
    {
        sonandoActualmente = false;

        if (!esAlarmaPostergeda)
        {
            horaPostergeda = horaAlarma;
            minutoPostergeda = minutoAlarma;
            esAlarmaPostergeda = true; // Activa el modo postergado
        }

        // Se suman 5 minutos exclusivamente a la variable de postergación
        minutoPostergeda += 5;
        if (minutoPostergeda >= 60)
        {
            minutoPostergeda -= 60;
            horaPostergeda = (horaPostergeda + 1) % 24;
        }

        ActualizarPantallaAlarma();
        DetenerAudio();
        OcultarBotonesDisparo();
    }

    private void DetenerAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void MostrarBotonesDisparo()
    {
        if (panelBotonesDisparo != null)
        {
            panelBotonesDisparo.SetActive(true);
        }
    }

    private void OcultarBotonesDisparo()
    {
        if (panelBotonesDisparo != null)
        {
            panelBotonesDisparo.SetActive(false);
        }
    }
}