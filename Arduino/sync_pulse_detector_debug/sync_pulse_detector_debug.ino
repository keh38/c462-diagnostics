#include <digitalWriteFast.h>

#define ANALOG_IN A0
#define TTL_PIN 13

const unsigned int SAMPLING_RATE = 50000; // Hz
const unsigned long SAMPLE_PERIOD = 1000000 / SAMPLING_RATE; // us

const float THRESHOLD_PERCENT = 0.025; // 2.5% threshold from mean
const int BIT_RESOLUTION = 10;  // 10 bit ADC/DAC resolution
const int MAX_VALUE = pow(2, BIT_RESOLUTION) - 1;  // max integer representation
const int MEAN_VALUE = (MAX_VALUE + 1) / 2; // middle value
const int THRESHOLD = MEAN_VALUE + THRESHOLD_PERCENT * MAX_VALUE; // middle + max(threshold)
const int PULSE_DUR = 1000;
const int REFRACTORY_PERIOD = 50000;

unsigned long lastSampleTime = 0;
bool aboveThreshold = false;
unsigned long lastTriggerTime = 0;
bool pulseHigh = false;

void setup() {
  Serial.begin(115200);
  // https://forum.arduino.cc/t/serial-readstring-is-extremely-slow/255110/25
  Serial.setTimeout(1);

  analogReadResolution(BIT_RESOLUTION);
  pinMode(ANALOG_IN, INPUT);
  pinMode(TTL_PIN, OUTPUT);
  digitalWriteFast(TTL_PIN, LOW);
}

void loop() {

  if (false && Serial.available() > 0)
  {
    unsigned long receiveTime = micros();
    String message = Serial.readString();
    if (message.startsWith("getlast"))
    {
      message.trim();
      message += lastTriggerTime;
      message += ";";
      message += receiveTime;
      message += ";";
      message += micros();
      Serial.println(message);
      lastTriggerTime = 0;
    }
    else if (message.startsWith("clear"))
    {
      lastTriggerTime = 0;
      pulseHigh = false;
      digitalWriteFast(TTL_PIN, LOW);
      Serial.println("OK");
    }
    else if (message.startsWith("'sup"))
    {
      Serial.println("livin' the dream:1.1");
    }
  }

  unsigned long currentTime = micros();
  
  if (currentTime - lastSampleTime >= SAMPLE_PERIOD) 
  {
    lastSampleTime = currentTime; 

    int data = analogRead(ANALOG_IN);
    Serial.println(data);

    // Check if signal crosses the threshold in the upward direction
    if (data > THRESHOLD && !aboveThreshold) 
    {
      aboveThreshold = true; 
      if (currentTime - lastTriggerTime > REFRACTORY_PERIOD)
      {
        lastTriggerTime = currentTime;
        digitalWriteFast(TTL_PIN, HIGH);
        pulseHigh = true;
      }
    }
    else if (aboveThreshold && data < MEAN_VALUE){//-THRESHOLD) {
      aboveThreshold = false;   // Reset tracking flag
    }

    if (pulseHigh && currentTime - lastTriggerTime > PULSE_DUR)
    {
      digitalWriteFast(TTL_PIN, LOW);
      pulseHigh = false;
    }
  }
}
