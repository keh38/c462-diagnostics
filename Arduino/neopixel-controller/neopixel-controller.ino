// NeoPixel Ring simple sketch (c) 2013 Shae Erisson
// released under the GPLv3 license to match the rest of the AdaFruit NeoPixel library

#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
  #include <avr/power.h>
#endif

// Which pin on the Arduino is connected to the NeoPixels?
// On a Trinket or Gemma we suggest changing this to 1
#define PIN            6

// How many NeoPixels are attached to the Arduino?
#define MAX_NUM_PIXELS     256

// When we setup the NeoPixel library, we tell it how many pixels, and which pin to use to send signals.
// Note that for older NeoPixel strips you might need to change the third parameter--see the strandtest
// example for more information on possible values.
Adafruit_NeoPixel pixels = Adafruit_NeoPixel(MAX_NUM_PIXELS, PIN, NEO_RGBW + NEO_KHZ800);

#define BAUD_RATE 9600     // (bits/second)          serial buffer baud rate
#define BUFF_SIZE 64       // (bytes)                maximum message size
#define MSG_TIMEOUT 5000   // (milliseconds)         timeout from last character received
#define NUM_CMDS 5

uint8_t buff[BUFF_SIZE];         // message read buffer
unsigned long lastCharTime;      // used to timeout a message that is cut off or too slow

uint8_t _red;
uint8_t _green;
uint8_t _blue;
uint8_t _white;

int _numPixels = 64;

// custom data type that is a pointer to a command function
typedef void (*CmdFunc)(int argc, char* argv[]);

// command structure
typedef struct {
  int commandArgs;          // number of command line arguments including the command string
  char* commandString;      // command string (e.g. "LED_ON", "LED_OFF"; use caps for alpha characters)
  CmdFunc commandFunction;  // pointer to the function that will execute if this command matches
} CmdStruct;

void Command_Clear(int argc = 0, char* argv[] = { NULL });
void Command_Identify(int argc = 0, char* argv[] = { NULL });
void Command_SetNumPixels(int argc = 0, char* argv[] = { NULL });
void Command_SetColor(int argc = 0, char* argv[] = { NULL });
void Command_GetColor(int argc = 0, char* argv[] = { NULL });

// command table
CmdStruct cmdTable[NUM_CMDS] = {
  {
    .commandArgs = 1,                  // CLEAR
    .commandString = "CLEAR",          // capitalized command for clearing display
    .commandFunction = &Command_Clear  // run Command_Clear
  },

  {
    .commandArgs = 1,                     // 'SUP
    .commandString = "'SUP",             // capitalized command for identifying self to host
    .commandFunction = &Command_Identify  // run Command_Identify
  },

  {
    .commandArgs = 2,                    // SETNUMPIXELS <N>
    .commandString = "SETNUMPIXELS",     // capitalized command for getting current colors
    .commandFunction = &Command_SetNumPixels // run Command_SetColor
  },

  {
    .commandArgs = 1,                    // GETCOLOR <R> <G> <B> <W>
    .commandString = "GETCOLOR",        // capitalized command for getting current colors
    .commandFunction = &Command_GetColor // run Command_SetColor
  },

  {
    .commandArgs = 5,                    // SETCOLOR <R> <G> <B> <W>
    .commandString = "SETCOLOR",        // capitalized command for adjusting LED color
    .commandFunction = &Command_SetColor // run Command_SetColor
  }
};

void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(BAUD_RATE);
  while (Serial.available() > 0) { Serial.read(); }
  Serial.flush();

  pixels.begin(); // This initializes the NeoPixel library.
  pixels.clear();
  pixels.show();
}

void loop() {
  // receive the serial input and process the message
  receive_message();
}

/******************************************************************************************
 **********************************   Helper Functions   **********************************
 ******************************************************************************************/

// reads in serial byte-by-byte and executes command
void receive_message(void) {

  // control variables
  uint8_t rc;              // stores byte from serial
  static uint8_t idx = 0;  // keeps track of which byte we are on

  // check for a timeout if we've received at least one character
  if (idx > 0) {
    // ignore message and reset index if we exceed timeout
    if ((millis() - lastCharTime) > MSG_TIMEOUT) {
      idx = 0;
    }
  }

  // if there's a character waiting
  if (Serial.available() > 0) {
    // update the last character timer
    lastCharTime = millis();
    // read the character
    rc = Serial.read();
    // if character is newline (serial monitor delimeter)
    if (rc == '\n') {
      // null-terminate the message
      buff[idx] = '\0';
      // and go process it
      process_message();
      // reset the buffer index to get ready for the next message
      idx = 0;
    } else {
      // store capitalized character and bump buffer pointer
      buff[idx++] = toupper(rc);
      // but not beyond the limits of the buffer
      if (idx == BUFF_SIZE) {
        --idx;
      }
    }
  }
}

// matches the message buffer to supported commands
void process_message(void) {

  // split the input message by a space delimeter (first token is the command name)
  char* token = strtok(buff, " ");

  // if we at least have a command name (first token)
  if (token != NULL) {
    // walk through command table to search for message match
    for (int i = 0; i < NUM_CMDS; ++i) {
      // start handling the arguments if the requested command is supported
      if (strcmp(token, cmdTable[i].commandString) == 0) {
        // get the number of required arguments
        int argc = cmdTable[i].commandArgs;
        // create an array to store arguments
        char* argv[argc];
        // store the command name in argv
        argv[0] = token;
        // parse the arguments required for the command
        for (int j = 1; j < argc; ++j) {
          // get the next argument
          token = strtok(NULL, " ");
          // check if there is too few arguments
          if (token == NULL) {
            return;
          }
          // store if it's provided
          argv[j] = token;
        }
        // try to get another argument (should be done already)
        token = strtok(NULL, " ");
        // check if there is too many arguments
        if (token != NULL) {
          return;
        }

        // send acknowledgement
        //Serial.write("OK\n");

        // execute the command and pass any arguments
        cmdTable[i].commandFunction(argc, argv);
        
        // ok get out of here now
        return;
      }
    }
  }
}

/******************************************************************************************
 *********************************   Command Functions   **********************************
 ******************************************************************************************/
// clear pixel strip
void Command_Clear(int argc, char* argv[]) {
  pixels.clear();
  pixels.show();

  Serial.write("OK\n");
}

// identify myself
void Command_Identify(int argc, char* argv[])
{
  Serial.write("lightin' the way, big man:3.0\n");
}

// change RGB values of LED
void Command_SetNumPixels(int argc, char* argv[]) {
  // ensure RGB arguments are byte sized
  _numPixels = atoi(argv[1]);
  
  Serial.write("OK\n");
}

// change RGB values of LED
void Command_SetColor(int argc, char* argv[]) {
  // ensure RGB arguments are byte sized
  _red = constrain(atoi(argv[1]), 0, 255);
  _green = constrain(atoi(argv[2]), 0, 255);
  _blue = constrain(atoi(argv[3]), 0, 255);
  _white = constrain(atoi(argv[4]), 0, 255);

  for(int i=0; i<_numPixels; i++){
    // pixels.Color takes RGB values, from 0,0,0 up to 255,255,255
    pixels.setPixelColor(i, pixels.Color(_green, _red, _blue, _white));
  }
  pixels.show(); // This sends the updated pixel color to the hardware.
  
  Serial.write("OK\n");
}

// Return current colors
void Command_GetColor(int argc, char* argv[])
{
  char response[80];
  sprintf(response, "OK %d %d %d %d\n", _red, _green, _blue, _white);
  Serial.write(response);
}
