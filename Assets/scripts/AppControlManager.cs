using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class AppControlManager : MonoBehaviour
{
    [Header("Botones de Control de Ventana")]
    [SerializeField] private Button btnMinimizar;
    [SerializeField] private Button btnCerrar;

    #if UNITY_STANDALONE_WIN && !UNITY_EDITOR

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    // Constantes Win32
    private const int SW_HIDE = 0;       // Oculta la ventana y la quita de la barra de tareas
    private const int SW_SHOW = 5;       // Muestra la ventana
    private const int SW_RESTORE = 9;    // Restaura la ventana a su tamaño original

    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;

    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000003;

    private const int IDI_APPLICATION = 32512;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    private IntPtr handleVentana;
    private NOTIFYICONDATA notifyData;
    private bool estaOculto = false;

    private void Start()
    {
        handleVentana = GetActiveWindow();
        InicializarTrayIcon();
    }

    private void OnDestroy()
    {
        EliminarTrayIcon();
    }

    private void InicializarTrayIcon()
    {
        notifyData = new NOTIFYICONDATA();
        notifyData.cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
        notifyData.hWnd = handleVentana;
        notifyData.uID = 1000;
        notifyData.uFlags = NIF_ICON | NIF_TIP;
        notifyData.hIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
        notifyData.szTip = "Reloj & Time Tracking";

        Shell_NotifyIcon(NIM_ADD, ref notifyData);
    }

    private void EliminarTrayIcon()
    {
        Shell_NotifyIcon(NIM_DELETE, ref notifyData);
    }

    #endif

    private void Awake()
    {
        if (btnMinimizar != null) btnMinimizar.onClick.AddListener(MinimizarABandeja);
        if (btnCerrar != null) btnCerrar.onClick.AddListener(CerrarAplicacion);
    }

    public void MinimizarABandeja()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (handleVentana == IntPtr.Zero)
        {
            handleVentana = GetActiveWindow();
        }

        // SW_HIDE remueve completamente el botón de la barra de tareas de Windows
        ShowWindow(handleVentana, SW_HIDE);
        estaOculto = true;
        #else
        Debug.Log("[AppControl] Ocultando de la barra de tareas (Comportamiento activo solo en Build ejecutable de Windows).");
        #endif
    }

    public void RestaurarVentana()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (handleVentana != IntPtr.Zero)
        {
            ShowWindow(handleVentana, SW_SHOW);
            ShowWindow(handleVentana, SW_RESTORE);
            estaOculto = false;
        }
        #endif
    }

    public void CerrarAplicacion()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        EliminarTrayIcon();
        #endif

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}