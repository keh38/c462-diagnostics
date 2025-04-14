#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
  #include <avr/power.h>
#endif
// Which pin on the Arduino is connected to the NeoPixels?
// On a Trinket or Gemma we suggest changing this to 1
#define PIN            6

// How many NeoPixels are attached to the Arduino?
#define NUM_LEDS      64

#define BAUD_RATE 115200  // (bits/second)          serial buffer baud rate
void Command_Clear();
void Command_SetColor(uint8_t R, uint8_t G, uint8_t B, uint8_t W);

uint8_t red;
uint8_t blue;
uint8_t green;
uint8_t white;

Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUM_LEDS, PIN, NEO_RGBW + NEO_KHZ800);

void setup() {
  Serial.begin(BAUD_RATE);
  // https://forum.arduino.cc/t/serial-readstring-is-extremely-slow/255110/25
  Serial.setTimeout(1);

  while (Serial.available() > 0) { Serial.read(); }
  Serial.flush();

  pixels.begin(); // This initializes the NeoPixel library.
  pixels.clear();
  pixels.show();
}

void loop() {
if (Serial.available() > 0)
  {
    String message = Serial.readString();
    if (message.startsWith("clear"))
    {
      Command_Clear();
    }
    else if (message.startsWith("set"))
    {
      int n = sscanf(message.c_str(), "set:%02hhX%02hhX%02hhX%02hhX", &red, &green, &blue, &white);
      Serial.println(n);
      Serial.println(red);
      Serial.println(green);
      Serial.println(blue);
      Serial.println(white);
      Command_SetColor(red, blue, green, white);
    }
    else if (message.startsWith("'sup"))
    {
      Serial.println("lightin' the way, big man:1.0");
    }
  }
}

/******************************************************************************************
 *********************************   Command Functions   **********************************
 ******************************************************************************************/

// clear pixel strip
void Command_Clear() {
  pixels.clear();
  pixels.show();

}

// change RGB values of LED
void Command_SetColor(uint8_t R, uint8_t G, uint8_t B, uint8_t W) {

  for(int i=0;i<NUM_LEDS;i++){
    // pixels.Color takes RGB values, from 0,0,0 up to 255,255,255
//    pixels.setPixelColor(i, pixels.Color(R, G, B, W));
    pixels.setPixelColor(i, pixels.Color(G,R,B,W));
  }
  pixels.show();
}
