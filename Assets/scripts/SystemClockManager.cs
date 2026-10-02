using System;
using UnityEngine;
using TMPro;

public class SystemClockManager : MonoBehaviour
{
    [Header("Referencias de UI (Panel Lateral / Encabezado)")]
    [SerializeField] private TextMeshProUGUI txtHoraSecundaria; // Ej: "10:00 AM"
    [SerializeField] private TextMeshProUGUI txtFecha;          // Ej: "06/10"
    [SerializeField] private TextMeshProUGUI txtAlarmaHeader;   // Ej: "ALARMA: 13:00 H"

    [Header("Configuración de Formato")]
    [SerializeField] private bool usarFormato12Horas = true;
    [SerializeField] private string textoAlarmaPorDefecto = "ALARMA: 13:00 H";

    // Propiedades públicas para consultar la hora formateada desde otros scripts
    public string HoraFormateadaActual { get; private set; }
    public string FechaFormateadaActual { get; private set; }

    private void Start()
    {
        if (txtAlarmaHeader != null)
        {
            txtAlarmaHeader.text = textoAlarmaPorDefecto;
        }
    }

    private void Update()
    {
        ActualizarTiempoSistema();
    }

    private void ActualizarTiempoSistema()
    {
        DateTime ahora = DateTime.Now;

        // Formato de hora (12h con AM/PM o 24h)
        HoraFormateadaActual = usarFormato12Horas 
            ? ahora.ToString("hh:mm tt") 
            : ahora.ToString("HH:mm");

        // Formato de fecha (Día/Mes)
        FechaFormateadaActual = ahora.ToString("dd/MM");

        // Actualizar UI del panel lateral
        if (txtHoraSecundaria != null)
        {
            txtHoraSecundaria.text = HoraFormateadaActual;
        }

        if (txtFecha != null)
        {
            txtFecha.text = FechaFormateadaActual;
        }
    }
}