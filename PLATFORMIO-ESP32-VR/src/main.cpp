#include <WiFi.h>
#include <WebSocketsServer.h>
#include <DHT.h>
#include <ArduinoJson.h>

//Wi-Fi
const char* ssid     = "MORAIS_2G";
const char* password = "202504mj23";

//Botões 
const int botaoVermelho = 19;
const int botaoAzul     = 5;
const int botaoBranco   = 4;
const int botaoAmarelo  = 23;

//LEDs 
const int ledVermelho = 33;
const int ledAzul     = 25;
const int ledBranco   = 12;
const int ledAmarelo  = 13;

//LED ESP32 de status conexã ----
const int ledStatus = 15;

//DHT-11
#define DHTPIN 27
#define DHTTYPE DHT11
DHT dht(DHTPIN, DHTTYPE);

const int botoes[4] = {botaoVermelho, botaoAzul, botaoBranco, botaoAmarelo};
const int leds[4]   = {ledVermelho, ledAzul, ledBranco, ledAmarelo};

WebSocketsServer ws(81);

//Eventos WebSocket
void onWebSocketEvent(uint8_t client, WStype_t type, uint8_t* payload, size_t length) {

    switch(type) {

        case WStype_CONNECTED:
            Serial.printf("Cliente %u conectado\n", client);
            digitalWrite(ledStatus, HIGH); // acende ao conectar
            break;

        case WStype_DISCONNECTED:
            Serial.printf("Cliente %u desconectado\n", client);
            digitalWrite(ledStatus, LOW); // apaga ao desconectar
            break;

        case WStype_TEXT: {
            StaticJsonDocument<128> doc;
            DeserializationError error = deserializeJson(doc, payload, length);

            if (error) {
                Serial.println("Erro ao ler JSON");
                return;
            }

            int ledIdx = doc["led"];
            int state  = doc["state"];

            if (ledIdx >= 0 && ledIdx < 4) {
                digitalWrite(leds[ledIdx], state ? HIGH : LOW);
            }
            break;
        }

        default:
            break;
    }
}

void setup() {
    Serial.begin(115200);
    dht.begin();

    for (int i = 0; i < 4; i++) {
        pinMode(botoes[i], INPUT_PULLUP);
        pinMode(leds[i], OUTPUT);
        digitalWrite(leds[i], LOW);
    }

    pinMode(ledStatus, OUTPUT);
    digitalWrite(ledStatus, LOW);

    WiFi.begin(ssid, password);

    Serial.print("Conectando Wi-Fi");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }

    Serial.println("\nWi-Fi conectado");
    Serial.print("IP: ");
    Serial.println(WiFi.localIP());

    ws.begin();
    ws.onEvent(onWebSocketEvent);

    Serial.println("WebSocket iniciado");
}

void loop() {
    ws.loop();

    static unsigned long lastSend = 0;

    if (millis() - lastSend > 2000) {
        lastSend = millis();

        float temp = dht.readTemperature();
        float umid = dht.readHumidity();

        StaticJsonDocument<128> doc;
        doc["temp"] = isnan(temp) ? 0 : temp;
        doc["umid"] = isnan(umid) ? 0 : umid;

        JsonArray btns = doc.createNestedArray("btns");

        for (int i = 0; i < 4; i++) {
            btns.add(digitalRead(botoes[i]) == LOW ? 1 : 0);
        }

        String json;
        serializeJson(doc, json);

        ws.broadcastTXT(json);
    }
}