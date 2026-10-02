using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppModeManager : MonoBehaviour
{
    public enum AppMode
    {
        TimeTracking,
        HoraActual,
        Alarma
    }
    [SerializeField] private TimeTrackingModule timeTrackingModule;
    [Header("Estado Actual")]
    [SerializeField] private AppMode modoActual = AppMode.TimeTracking;

    [Header("Referencias de UI Central")]
    [SerializeField] private TextMeshProUGUI txtDisplayCentral;
    [SerializeField] private TextMeshProUGUI txtEtiquetaModo;
    [SerializeField] private GameObject dropdownTono;

    [Header("Referencias de UI Secundaria")]
    [SerializeField] private GameObject objetoHoraSecundaria;

    [Header("Contenedores de Botoneras (Jerarquía)")]
    [SerializeField] private GameObject panelBotonesAlarma; // Objeto "botones alarma"
    [SerializeField] private GameObject panelBotonesTime;   // Objeto "buttons_time"

    [Header("Botones y Controladores")]
    [SerializeField] private Button btnMode;
    [SerializeField] private SystemClockManager systemClockManager;

    [Header("Estilos de Etiqueta por Modo")]
    [SerializeField] private Color colorTextoTimeTracking = Color.black;
    [SerializeField] private Color colorTextoHoraActual = new Color(0.7f, 0f, 0f);
    [SerializeField] private Color colorTextoAlarma = new Color(0.9f, 0.5f, 0f);

    private string horaAlarmaProgramada = "13:00:00";

    private void Awake()
    {
        if (btnMode != null)
        {
            btnMode.onClick.AddListener(CambiarModo);
        }
    }

    private void Start()
    {
        AplicarModoUI(modoActual);
    }

    private void Update()
    {
        if (modoActual == AppMode.HoraActual && systemClockManager != null && txtDisplayCentral != null)
        {
            txtDisplayCentral.text = systemClockManager.HoraFormateadaActual;
        }
    }

    public void CambiarModo()
    {
        modoActual = modoActual switch
        {
            AppMode.TimeTracking => AppMode.HoraActual,
            AppMode.HoraActual => AppMode.Alarma,
            AppMode.Alarma => AppMode.TimeTracking,
            _ => AppMode.TimeTracking
        };

        AplicarModoUI(modoActual);
    }

    private void AplicarModoUI(AppMode modo)
    {
        bool esModoAlarma = (modo == AppMode.Alarma);
        bool esModoTimeTracking = (modo == AppMode.TimeTracking);

        // Control del Dropdown de Tonos
        if (dropdownTono != null)
        {
            dropdownTono.SetActive(esModoAlarma);
        }

        // Conmutar contenedores de botones completos
        if (panelBotonesAlarma != null) panelBotonesAlarma.SetActive(esModoAlarma);
        if (panelBotonesTime != null) panelBotonesTime.SetActive(esModoTimeTracking);

        switch (modo){
            
            case AppMode.TimeTracking:
        if (txtEtiquetaModo != null)
        {
            txtEtiquetaModo.text = "TIME TRACKING";
            txtEtiquetaModo.color = colorTextoTimeTracking;
        }

        if (objetoHoraSecundaria != null)
        {
            objetoHoraSecundaria.SetActive(true);
        }

        if (timeTrackingModule != null)
        {
            timeTrackingModule.ActualizarDisplayUI();
        }
         break;

        case AppMode.HoraActual:
             if (txtEtiquetaModo != null)
                {
                    txtEtiquetaModo.text = "hora actual";
                    txtEtiquetaModo.color = colorTextoHoraActual;
                }

                if (objetoHoraSecundaria != null)
                {
                    objetoHoraSecundaria.SetActive(false);
                }

                if (systemClockManager != null && txtDisplayCentral != null)
                {
                    txtDisplayCentral.text = systemClockManager.HoraFormateadaActual;
                }
                break;

            case AppMode.Alarma:
                if (txtEtiquetaModo != null)
                {
                    txtEtiquetaModo.text = "ALARMA";
                    txtEtiquetaModo.color = colorTextoAlarma;
                }

                if (objetoHoraSecundaria != null)
                {
                    objetoHoraSecundaria.SetActive(true);
                }

                if (txtDisplayCentral != null)
                {
                    txtDisplayCentral.text = horaAlarmaProgramada;
                }
                break;
        }
    }

    public AppMode ObtenerModoActual() => modoActual;

    public void ActualizarHoraAlarmaDisplay(string nuevaHora)
    {
        horaAlarmaProgramada = nuevaHora;
        if (modoActual == AppMode.Alarma && txtDisplayCentral != null)
        {
            txtDisplayCentral.text = horaAlarmaProgramada;
        }
    }
}