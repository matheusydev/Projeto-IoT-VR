# ESP32 VR Monitor — Integração IoT com Realidade Virtual

> Projeto de integração entre hardware embarcado (ESP32) e ambiente de Realidade Virtual (Meta Quest 2), permitindo visualizar dados de sensores e interagir com LEDs físicos em tempo real dentro de uma cena VR.

---

## Sumário

- [Descrição Geral](#descrição-geral)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Arquitetura do Sistema](#arquitetura-do-sistema)
- [Hardware Utilizado](#hardware-utilizado)
- [Pinagem do ESP32](#pinagem-do-esp32)
- [Funcionamento do Firmware ESP32](#funcionamento-do-firmware-esp32)
- [Comunicação ESP32 ↔ Unity](#comunicação-esp32--unity)
- [Interface VR na Unity](#interface-vr-na-unity)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Como Executar o Projeto](#como-executar-o-projeto)

---

## Descrição Geral

Este projeto demonstra a integração entre um microcontrolador **ESP32** e um ambiente de **Realidade Virtual** executado no headset **Meta Quest 2**, utilizando a engine **Unity** como plataforma de desenvolvimento.

O ESP32 é responsável por coletar dados do ambiente físico — temperatura e umidade via sensor DHT11 — e monitorar o estado de 4 botões físicos. Esses dados são transmitidos em tempo real via **WebSocket sobre Wi-Fi** para a aplicação Unity, que os exibe em um painel interativo dentro da cena VR.

O usuário, dentro do ambiente virtual, visualiza as leituras do sensor atualizadas a cada 2 segundos e enxerga quatro esferas coloridas que acendem ou apagam conforme os botões físicos são pressionados ou soltos — criando uma ponte direta entre o mundo físico e o virtual.

### Aplicações práticas

- Monitoramento ambiental em ambientes imersivos de treinamento
- Protótipos de dashboards industriais em VR
- Demonstração de conceitos de IoT integrada com interfaces XR
- Base para projetos de automação com feedback visual em realidade virtual

---

## Tecnologias Utilizadas

### Hardware

| Tecnologia | Descrição |
|---|---|
| **ESP32** | Microcontrolador com Wi-Fi integrado responsável pela leitura dos sensores, controle dos LEDs e comunicação via WebSocket |
| **DHT11** | Sensor digital de temperatura e umidade |
| **Botões** | 4 botões físicos que enviam sinais digitais ao ESP32 |
| **LEDs** | 4 LEDs coloridos controlados tanto pelo firmware quanto pela Unity via WebSocket |

### Software — ESP32

| Tecnologia | Descrição |
|---|---|
| **PlatformIO** | Ambiente de desenvolvimento para firmware embarcado, integrado ao VS Code |
| **Arduino Framework** | Framework utilizado na programação do ESP32 |
| **WebSockets** (links2004) | Biblioteca para servidor WebSocket no ESP32 |
| **ArduinoJson** (bblanchon) | Serialização e desserialização de dados no formato JSON |
| **DHT sensor library** (Adafruit) | Leitura do sensor DHT11 |

### Software — Unity / VR

| Tecnologia | Descrição |
|---|---|
| **Unity** | Engine de desenvolvimento do ambiente VR |
| **Meta Quest 2** | Headset de realidade virtual onde a aplicação é executada |
| **Meta XR SDK** | SDK oficial da Meta para desenvolvimento VR no Unity |
| **NativeWebSocket** | Biblioteca C# para comunicação WebSocket no Unity |
| **TextMeshPro** | Sistema de renderização de texto para a interface VR |
| **UnityMainThreadDispatcher** | Utilitário que permite atualizar a cena Unity a partir de threads externas |

---

## Arquitetura do Sistema

O sistema é dividido em duas camadas que se comunicam via rede local Wi-Fi utilizando o protocolo WebSocket.

```
┌─────────────────────────────────────┐
│              ESP32                  │
│                                     │
│  DHT11 ──► Leitura temp/umidade     │
│  Botões ──► Leitura digital         │
│                                     │
│  Serializa dados em JSON            │
│  Servidor WebSocket (porta 81)      │
└────────────────┬────────────────────┘
                 │
                 │  Wi-Fi (rede local)
                 │  WebSocket — JSON
                 │
                 ▼
┌─────────────────────────────────────┐
│           Unity / Meta Quest 2      │
│                                     │
│  Cliente WebSocket                  │
│  Desserializa JSON recebido         │
│                                     │
│  Atualiza textos (temp, umidade)    │
│  Atualiza esferas (botões)          │
│                                     │
│  Interface VR exibida no headset    │
└─────────────────────────────────────┘
```

### Fluxo de dados

1. O ESP32 inicializa, conecta ao Wi-Fi e sobe um servidor WebSocket na porta 81
2. A Unity, ao iniciar a cena, conecta como cliente WebSocket usando o IP do ESP32
3. A cada 2 segundos, o ESP32 lê o DHT11 e os 4 botões, serializa tudo em JSON e faz broadcast para todos os clientes conectados
4. A Unity recebe o JSON, desserializa e atualiza os elementos visuais da cena em tempo real
5. Quando um botão físico é pressionado, a esfera correspondente acende na cor configurada; ao soltar, apaga
6. A Unity também pode enviar comandos ao ESP32 para acender ou apagar os LEDs físicos remotamente

---

## Hardware Utilizado

| Componente | Quantidade | Função no Projeto |
|---|---|---|
| ESP32 DevKit V1 | 1 | Microcontrolador principal — Wi-Fi, leitura de sensores, controle de LEDs |
| Sensor DHT11 | 1 | Medição de temperatura e umidade relativa do ar |
| Botão táctil | 4 | Entrada digital — cada botão controla uma esfera na cena VR |
| LED vermelho | 1 | Indicador visual físico — controlado pelo botão vermelho / Unity |
| LED azul | 1 | Indicador visual físico — controlado pelo botão azul / Unity |
| LED branco | 1 | Indicador visual físico — controlado pelo botão branco / Unity |
| LED amarelo | 1 | Indicador visual físico — controlado pelo botão amarelo / Unity |
| LED de status | 1 | Indica se há um cliente WebSocket conectado |
| Resistores 220Ω | 4 | Proteção dos LEDs contra sobrecorrente |
| Protoboard | 1 | Montagem do circuito |
| Cabos jumper | — | Conexões entre componentes |
| Resistores 10kΩ | 4 | Pull-up externo opcional para os botões |

---

## Pinagem do ESP32

| GPIO | Componente | Direção | Função |
|---|---|---|---|
| GPIO 27 | DHT11 (Data) | Entrada | Leitura de temperatura e umidade |
| GPIO 19 | Botão Vermelho | Entrada (PULLUP) | Pressionar acende esfera vermelha na VR |
| GPIO 5 | Botão Azul | Entrada (PULLUP) | Pressionar acende esfera azul na VR |
| GPIO 4 | Botão Branco | Entrada (PULLUP) | Pressionar acende esfera branca na VR |
| GPIO 23 | Botão Amarelo | Entrada (PULLUP) | Pressionar acende esfera amarela na VR |
| GPIO 33 | LED Vermelho | Saída | Controlado pelo botão vermelho e pela Unity |
| GPIO 25 | LED Azul | Saída | Controlado pelo botão azul e pela Unity |
| GPIO 12 | LED Branco | Saída | Controlado pelo botão branco e pela Unity |
| GPIO 13 | LED Amarelo | Saída | Controlado pelo botão amarelo e pela Unity |
| GPIO 2 | LED Status | Saída | Acende quando um cliente WebSocket se conecta |


---

## Funcionamento do Firmware ESP32

### Conexão Wi-Fi

O firmware inicia conectando ao roteador configurado nas variáveis `ssid` e `password`. O processo é bloqueante — o ESP32 aguarda até obter IP antes de continuar. O IP atribuído é exibido no Serial Monitor e deve ser anotado para configurar na Unity.

### Servidor WebSocket

Após conectar ao Wi-Fi, o ESP32 sobe um servidor WebSocket na porta **81**. Esse servidor gerencia eventos de conexão, desconexão e recebimento de mensagens:

- **Conexão de cliente** — acende o LED de status (GPIO 15)
- **Desconexão de cliente** — apaga o LED de status
- **Mensagem recebida** — interpreta o JSON e acende/apaga o LED físico correspondente

### Leitura dos sensores e envio de dados

A cada **2 segundos**, o firmware executa:

1. Leitura da temperatura e umidade via DHT11
2. Leitura do estado dos 4 botões (`LOW` = pressionado com PULLUP interno)
3. Serialização em JSON com o formato abaixo
4. Broadcast para todos os clientes WebSocket conectados

```json
{
  "temp": 28.5,
  "umid": 65.0,
  "btns": [0, 1, 0, 0]
}
```

O array `btns` contém 4 valores onde `1` significa botão pressionado e `0` significa solto. A ordem é sempre: vermelho, azul, branco, amarelo.

### Lógica dos botões

Os botões são configurados com `INPUT_PULLUP`, o que significa que o pino lê `HIGH` quando o botão está solto e `LOW` quando pressionado. O firmware inverte essa lógica para enviar `1` ao pressionar e `0` ao soltar, tornando o JSON mais intuitivo.

---

## Comunicação ESP32 ↔ Unity

### Conexão

O script `ESP32Manager.cs` na Unity cria um cliente WebSocket ao iniciar a cena, conectando ao endereço `ws://[IP_DO_ESP32]:81`. O IP é configurável diretamente no Inspector do Unity sem precisar recompilar o código.

### Recebimento de dados

As mensagens chegam como bytes e são convertidas para string UTF-8. A classe `ESP32Data` define o molde exato do JSON esperado, e o `JsonUtility.FromJson` converte automaticamente o texto em objeto C#:

```csharp
[System.Serializable]
class ESP32Data {
    public float temp;
    public float umid;
    public int[] btns;
}
```

Como o WebSocket opera em uma thread separada e o Unity só permite modificar objetos da cena na thread principal, o `UnityMainThreadDispatcher` enfileira as atualizações visuais para execução segura no próximo frame.

### Envio de comandos para os LEDs

A Unity também pode enviar comandos ao ESP32 para controlar os LEDs físicos. O JSON enviado segue o formato:

```json
{ "led": 0, "state": 1 }
```

Onde `led` é o índice (0 = vermelho, 1 = azul, 2 = branco, 3 = amarelo) e `state` é `1` para acender e `0` para apagar.

### Exemplo de troca de mensagens

```
ESP32  ──►  Unity   {"temp":28.5,"umid":65.0,"btns":[0,0,1,0]}
Unity  ──►  ESP32   {"led":2,"state":1}
ESP32  ──►  Unity   {"temp":28.6,"umid":64.8,"btns":[0,0,0,0]}
```

---

## Interface VR na Unity

### Elementos visuais

A cena VR contém os seguintes elementos:

| Elemento | Tipo Unity | Descrição |
|---|---|---|
| Painel de temperatura | TextMeshPro UGUI | Exibe a leitura atual em °C com 1 casa decimal |
| Painel de umidade | TextMeshPro UGUI | Exibe a leitura atual em % com 1 casa decimal |
| Esfera Vermelha | GameObject + Renderer | Representa o botão vermelho físico |
| Esfera Azul | GameObject + Renderer | Representa o botão azul físico |
| Esfera Branca | GameObject + Renderer | Representa o botão branco físico |
| Esfera Amarela | GameObject + Renderer | Representa o botão amarelo físico |

### Comportamento das esferas

As esferas funcionam como indicadores de estado em tempo real:

| Estado do botão | Cor da esfera |
|---|---|
| Solto | Quase preto `(0.08, 0.08, 0.12)` — apagada |
| Pressionado | Cor viva correspondente — acesa |

As cores configuradas por padrão são:

| Esfera | Cor ligado | Representação |
|---|---|---|
| Vermelha | `(1.0, 0.15, 0.15)` | Vermelho vivo |
| Azul | `(0.2, 0.5, 1.0)` | Azul claro |
| Branca | `(1.0, 1.0, 1.0)` | Branco puro |
| Amarela | `(1.0, 0.85, 0.1)` | Amarelo vivo |

Todas as cores são ajustáveis diretamente no Inspector do Unity sem necessidade de alterar o código.

### Atualização dos dados

Os textos de temperatura e umidade são atualizados a cada mensagem recebida do ESP32 (intervalo de 2 segundos). As esferas atualizam instantaneamente ao receber qualquer mudança no estado dos botões, com verificação de estado anterior para evitar redesenho desnecessário.

---

## Estrutura do Projeto

```
esp32-vr-monitor/
│
├── ESP32/
│   ├── src/
│   │   └── main.cpp              # Firmware principal do ESP32
│   ├── include/                  # Headers adicionais (se necessário)
│   └── platformio.ini            # Configuração do projeto PlatformIO
│
├── Unity/
│   └── Assets/
│       ├── Scripts/
│       │   ├── ESP32Manager.cs             # Gerencia conexão WebSocket e atualiza a cena
│       │   └── UnityMainThreadDispatcher.cs # Executa ações na thread principal do Unity
│       ├── Scenes/
│       │   └── SampleScene.unity           # Cena principal com o painel VR
│       └── Materials/                      # Materiais das esferas (opcional)
│
└── README.md
```

---

## Como Executar o Projeto

### Pré-requisitos

- VS Code com extensão PlatformIO instalada
- Unity 2022.3 LTS ou superior
- Meta Quest 2 com modo desenvolvedor ativado
- Pacote **Meta XR All-in-One SDK** instalado no Unity
- Pacote **NativeWebSocket** instalado no Unity via Package Manager
- ESP32 e componentes montados conforme a tabela de pinagem
- ESP32 e headset Meta Quest 2 na **mesma rede Wi-Fi**

---

### Passo 1 — Configurar o Wi-Fi no ESP32

Abra o arquivo `ESP32/src/main.cpp` e preencha suas credenciais de rede:

```cpp
const char* ssid     = "NOME_DA_SUA_REDE";
const char* password = "SENHA_DA_SUA_REDE";
```

---

### Passo 2 — Compilar e enviar o firmware

1. Abra a pasta `ESP32/` no VS Code com PlatformIO
2. Conecte o ESP32 via USB
3. Clique em **Upload** (ícone de seta) na barra inferior do PlatformIO
4. Aguarde a compilação e o envio

---

### Passo 3 — Identificar o IP do ESP32

1. Após o upload, abra o **Serial Monitor** no PlatformIO (ícone de tomada)
2. Configure o baud rate para `115200`
3. Pressione o botão **EN/Reset** na placa
4. Anote o IP exibido, por exemplo: `IP: 192.168.1.42`

---

### Passo 4 — Configurar o IP na Unity

1. Abra o projeto Unity
2. Na **Hierarchy**, selecione o objeto `ESP32Manager`
3. No **Inspector**, localize o campo `Esp IP`
4. Substitua pelo IP anotado no passo anterior

---

### Passo 5 — Configurar as referências no Inspector

Com o objeto `ESP32Manager` selecionado, arraste os elementos da cena para os campos:

| Campo | Objeto a arrastar |
|---|---|
| Temp Text | TextMeshPro da temperatura no Canvas |
| Umid Text | TextMeshPro da umidade no Canvas |
| Led Indicators / Element 0 | Esfera vermelha |
| Led Indicators / Element 1 | Esfera azul |
| Led Indicators / Element 2 | Esfera branca |
| Led Indicators / Element 3 | Esfera amarela |

---

### Passo 6 — Testar no Editor antes do headset

1. Clique em **Play** no Unity Editor
2. Abra o **Console** (Window → Console)
3. Verifique se aparece `Conectado ao ESP32!`
4. Pressione os botões físicos e observe as esferas mudando de cor na Game View

---

### Passo 7 — Build para o Meta Quest 2

1. Vá em **File → Build Settings**
2. Selecione a plataforma **Android**
3. Conecte o Meta Quest 2 via USB com modo desenvolvedor ativo
4. Clique em **Build and Run**
5. Aceite a permissão de instalação no headset

---

> **ATENÇÃO:** Certifique-se de que o Meta Quest 2 está conectado à mesma rede Wi-Fi do ESP32 antes de abrir a aplicação no headset. A comunicação WebSocket só funciona em rede local.