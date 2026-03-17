// NeoPixel Unified RGB / RGBW Sketch (Dynamic Object Version)

#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
#include <avr/power.h>
#endif

#define PIN 5
#define MAX_NUM_PIXELS 256

Adafruit_NeoPixel* pixels = nullptr;

#define BAUD_RATE 9600
#define BUFF_SIZE 64
#define MSG_TIMEOUT 5000
#define NUM_CMDS 6

uint8_t buff[BUFF_SIZE];
unsigned long lastCharTime;

uint8_t _red;
uint8_t _green;
uint8_t _blue;
uint8_t _white;

int _numPixels = 64;

enum LedType
{
  LED_RGB,
  LED_RGBW
};

LedType currentLedType = LED_RGBW;


// command function pointer
typedef void (*CmdFunc)(int argc, char* argv[]);

// command structure
typedef struct {
  int commandArgs;
  char* commandString;
  CmdFunc commandFunction;
} CmdStruct;


// Command prototypes
void Command_Clear(int argc = 0, char* argv[] = { NULL });
void Command_Identify(int argc = 0, char* argv[] = { NULL });
void Command_SetNumPixels(int argc = 0, char* argv[] = { NULL });
void Command_SetLedType(int argc = 0, char* argv[] = { NULL });
void Command_SetColor(int argc = 0, char* argv[] = { NULL });
void Command_GetColor(int argc = 0, char* argv[] = { NULL });


// Command table
CmdStruct cmdTable[NUM_CMDS] = {

  {1,"CLEAR",&Command_Clear},
  {1,"'SUP",&Command_Identify},
  {2,"SETNUMPIXELS",&Command_SetNumPixels},
  {2,"SETLEDTYPE",&Command_SetLedType},
  {1,"GETCOLOR",&Command_GetColor},
  {5,"SETCOLOR",&Command_SetColor}

};


void initPixels(uint16_t type)
{
  if(pixels != nullptr)
  {
    delete pixels;
  }

  pixels = new Adafruit_NeoPixel(MAX_NUM_PIXELS, PIN, type);

  pixels->begin();
  pixels->clear();
  pixels->show();
}



void setup()
{
  Serial.begin(BAUD_RATE);

  while (Serial.available() > 0) { Serial.read(); }
  Serial.flush();

  // Default to RGBW strip
  initPixels(NEO_RGBW + NEO_KHZ800);
}


void loop()
{
  receive_message();
}



/******************************************************************************************
 **********************************   Helper Functions   **********************************
 ******************************************************************************************/

void receive_message(void)
{

  uint8_t rc;
  static uint8_t idx = 0;

  if (idx > 0)
  {
    if ((millis() - lastCharTime) > MSG_TIMEOUT)
    {
      idx = 0;
    }
  }

  if (Serial.available() > 0)
  {
    lastCharTime = millis();

    rc = Serial.read();

    if (rc == '\n')
    {
      buff[idx] = '\0';
      process_message();
      idx = 0;
    }
    else
    {
      buff[idx++] = toupper(rc);

      if (idx == BUFF_SIZE)
      {
        --idx;
      }
    }
  }
}



void process_message(void)
{

  char* token = strtok(buff, " ");

  if (token != NULL)
  {
    for (int i = 0; i < NUM_CMDS; ++i)
    {

      if (strcmp(token, cmdTable[i].commandString) == 0)
      {

        int argc = cmdTable[i].commandArgs;

        char* argv[argc];

        argv[0] = token;

        for (int j = 1; j < argc; ++j)
        {

          token = strtok(NULL, " ");

          if (token == NULL)
          {
            return;
          }

          argv[j] = token;
        }

        token = strtok(NULL, " ");

        if (token != NULL)
        {
          return;
        }

        cmdTable[i].commandFunction(argc, argv);

        return;
      }
    }
  }
}



/******************************************************************************************
 *********************************   Command Functions   **********************************
 ******************************************************************************************/

void Command_Clear(int argc, char* argv[])
{
  if(pixels == nullptr) return;

  pixels->clear();
  pixels->show();

  Serial.write("OK\n");
}



void Command_Identify(int argc, char* argv[])
{
  Serial.write("lightin' the way, big man:4.0\n");
}



void Command_SetNumPixels(int argc, char* argv[])
{
  _numPixels = atoi(argv[1]);

  if(_numPixels > MAX_NUM_PIXELS)
    _numPixels = MAX_NUM_PIXELS;

  Serial.write("OK\n");
}



void Command_SetLedType(int argc, char* argv[])
{

  if(strcmp(argv[1],"RGBW")==0)
  {
    currentLedType = LED_RGBW;
    initPixels(NEO_RGBW + NEO_KHZ800);
  }
  else if(strcmp(argv[1],"RGB")==0)
  {
    currentLedType = LED_RGB;
    initPixels(NEO_GRB + NEO_KHZ800);
  }
  else
  {
    Serial.write("ERR\n");
    return;
  }

  Serial.write("OK\n");
}



void Command_SetColor(int argc, char* argv[])
{

  if(pixels == nullptr) return;

  _red   = constrain(atoi(argv[1]),0,255);
  _green = constrain(atoi(argv[2]),0,255);
  _blue  = constrain(atoi(argv[3]),0,255);
  _white = constrain(atoi(argv[4]),0,255);

  for(int i=0;i<_numPixels;i++)
  {

    if(currentLedType == LED_RGBW)
    {
      pixels->setPixelColor(i, pixels->Color(_green,_red,_blue,_white));
    }
    else
    {
      pixels->setPixelColor(i, pixels->Color(_red,_green,_blue));
    }

  }

  pixels->show();

  Serial.write("OK\n");
}



void Command_GetColor(int argc, char* argv[])
{

  char response[80];

  sprintf(response,"OK %d %d %d %d\n",_red,_green,_blue,_white);

  Serial.write(response);

}