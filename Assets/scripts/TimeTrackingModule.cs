using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeTrackingModule : MonoBehaviour
{
    [Header("Botones de Control")]
    [SerializeField] private Button btnPlayPause;   // Botón 1 (Play / Pause Dual)
    [SerializeField] private Button btnStop;        // Botón 2 (Stop / Reiniciar sesión)
    [SerializeField] private Button btnSave;        // Botón 3 (SAVE / Sumar a Anterior)
    [SerializeField] private Button btnTimeClear;   // Botón 4 (TIME CLEAR)

    [Header("UI Textos")]
    [SerializeField] private TextMeshProUGUI txtDisplayCentral;
    [SerializeField] private TextMeshProUGUI txtTiempoAnterior; // Texto del panel lateral (ANTERIOR)

    [Header("Dependencias")]
    [SerializeField] private AppModeManager modeManager;

    private const string PREF_TOTAL_SEGUNDOS = "TimeTracking_TotalSegundos";

    private enum EstadoTimer
    {
        Detenido,
        Corriendo,
        Pausado
    }

    private EstadoTimer estadoActual = EstadoTimer.Detenido;
    private float tiempoSesionActual = 0f;
    private double totalSegundosAcumulados = 0;

    private void Awake()
    {
        if (btnPlayPause != null) btnPlayPause.onClick.AddListener(TogglePlayPause);
        if (btnStop != null) btnStop.onClick.AddListener(DetenerYReiniciarSesion);
        if (btnSave != null) btnSave.onClick.AddListener(GuardarYAcumular);
        if (btnTimeClear != null) btnTimeClear.onClick.AddListener(BorrarTiempoAcumulado);
    }

    private void Start()
    {
        CargarTiempoAcumulado();
        ActualizarDisplayCentral();
    }

    private void Update()
    {
        if (estadoActual == EstadoTimer.Corriendo)
        {
            tiempoSesionActual += Time.deltaTime;

            if (modeManager != null && modeManager.ObtenerModoActual() == AppModeManager.AppMode.TimeTracking)
            {
                ActualizarDisplayCentral();
            }
        }
    }

    /// <summary>
    /// Alterna entre Play y Pause con el mismo botón.
    /// </summary>
    public void TogglePlayPause()
    {
        if (modeManager != null && modeManager.ObtenerModoActual() != AppModeManager.AppMode.TimeTracking) return;

        if (estadoActual == EstadoTimer.Corriendo)
        {
            estadoActual = EstadoTimer.Pausado;
        }
        else
        {
            estadoActual = EstadoTimer.Corriendo;
        }
    }

    /// <summary>
    /// Detiene y reinicia a 0 el contador de la sesión actual sin acumularlo.
    /// </summary>
    public void DetenerYReiniciarSesion()
    {
        if (modeManager != null && modeManager.ObtenerModoActual() != AppModeManager.AppMode.TimeTracking) return;

        estadoActual = EstadoTimer.Detenido;
        tiempoSesionActual = 0f;
        ActualizarDisplayCentral();
    }

    /// <summary>
    /// Suma el tiempo de la sesión actual al total en "ANTERIOR", guarda en PlayerPrefs y reinicia la sesión.
    /// </summary>
    public void GuardarYAcumular()
    {
        if (modeManager != null && modeManager.ObtenerModoActual() != AppModeManager.AppMode.TimeTracking) return;

        totalSegundosAcumulados += tiempoSesionActual;
        tiempoSesionActual = 0f;
        estadoActual = EstadoTimer.Detenido;

        GuardarConfiguracion();
        ActualizarDisplayCentral();
        ActualizarDisplayAnterior();
    }

    /// <summary>
    /// Resetea a 00:00:00 el total acumulado en "ANTERIOR" y limpia PlayerPrefs.
    /// </summary>
    public void BorrarTiempoAcumulado()
    {
        if (modeManager != null && modeManager.ObtenerModoActual() != AppModeManager.AppMode.TimeTracking) return;

        totalSegundosAcumulados = 0;
        GuardarConfiguracion();
        ActualizarDisplayAnterior();
    }

    public void ActualizarDisplayUI()
    {
        ActualizarDisplayCentral();
        ActualizarDisplayAnterior();
    }

    private void GuardarConfiguracion()
    {
        PlayerPrefs.SetString(PREF_TOTAL_SEGUNDOS, totalSegundosAcumulados.ToString());
        PlayerPrefs.Save();
    }

    private void CargarTiempoAcumulado()
    {
        string guardado = PlayerPrefs.GetString(PREF_TOTAL_SEGUNDOS, "0");
        if (double.TryParse(guardado, out double valor))
        {
            totalSegundosAcumulados = valor;
        }
        else
        {
            totalSegundosAcumulados = 0;
        }

        ActualizarDisplayAnterior();
    }

    private void ActualizarDisplayCentral()
    {
        if (txtDisplayCentral != null)
        {
            txtDisplayCentral.text = FormatearTiempo(tiempoSesionActual);
        }
    }

    private void ActualizarDisplayAnterior()
    {
        if (txtTiempoAnterior != null)
        {
            txtTiempoAnterior.text = FormatearTiempo(totalSegundosAcumulados);
        }
    }

    private string FormatearTiempo(double segundosTotales)
    {
        TimeSpan t = TimeSpan.FromSeconds(segundosTotales);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)t.TotalHours, t.Minutes, t.Seconds);
    }
}