using System.Collections;
using UnityEngine;
using TMPro;
using NativeWebSocket;

public class ESP32Manager : MonoBehaviour
{
    [Header("Endereço do ESP32")]
    public string espIP = "";

    [Header("UI")]
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI umidText;

    [Header("Esferas (ordem: Vermelho, Azul, Branco, Amarelo)")]
    public GameObject[] ledIndicators; // 4 esferas

    [Header("Cor LIGADO de cada esfera")]
    public Color corLigadoVermelho = new Color(1f, 0.15f, 0.15f); // vermelho vivo
    public Color corLigadoAzul = new Color(0.2f, 0.5f, 1f);    // azul claro
    public Color corLigadoBranco = new Color(1f, 1f, 1f);    // branco
    public Color corLigadoAmarelo = new Color(1f, 0.85f, 0.1f);  // amarelo vivo

    [Header("Cor DESLIGADO (igual para todas)")]
    public Color corDesligado = new Color(0.08f, 0.08f, 0.12f); // quase preto

    private Color[] coresLigado;
    private int[] estadoAnterior = new int[4];

    WebSocket ws;

    void Awake()
    {
        coresLigado = new Color[]
        {
            corLigadoVermelho,
            corLigadoAzul,
            corLigadoBranco,
            corLigadoAmarelo
        };

        // Começa todas apagadas
        for (int i = 0; i < 4; i++)
            SetCor(i, corDesligado);
    }

    async void Start()
    {
        Debug.Log("Conectando em: ws://" + espIP + ":81");
        ws = new WebSocket($"ws://{espIP}:81");

        ws.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Recebido: " + json);
            HandleMessage(json);
        };

        ws.OnOpen += () => Debug.Log("Conectado ao ESP32!");
        ws.OnError += (e) => Debug.Log("Erro: " + e);
        ws.OnClose += (c) => Debug.Log("Desconectado");

        await ws.Connect();
    }

    void HandleMessage(string json)
    {
        var data = JsonUtility.FromJson<ESP32Data>(json);
        if (data == null || data.btns == null) return;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Temperatura e umidade
            if (tempText) tempText.text = $"Temp: {data.temp:F1} °C";
            if (umidText) umidText.text = $"Umid: {data.umid:F1} %";

            // Atualiza cada esfera conforme estado do botão
            for (int i = 0; i < 4 && i < ledIndicators.Length; i++)
            {
                int estado = (data.btns.Length > i) ? data.btns[i] : 0;

                // Só atualiza cor se o estado mudou (evita set desnecessário a cada 2s)
                if (estado != estadoAnterior[i])
                {
                    SetCor(i, estado == 1 ? coresLigado[i] : corDesligado);
                    estadoAnterior[i] = estado;
                }
            }
        });
    }

    void SetCor(int index, Color cor)
    {
        if (index >= ledIndicators.Length || ledIndicators[index] == null) return;
        var rend = ledIndicators[index].GetComponent<Renderer>();
        if (rend != null) rend.material.color = cor;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
    }

    async void OnDestroy()
    {
        if (ws != null)
            await ws.Close();
    }

    [System.Serializable]
    class ESP32Data
    {
        public float temp;
        public float umid;
        public int[] btns;
    }
}