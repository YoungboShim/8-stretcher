#include <Wire.h>
#include <Adafruit_PWMServoDriver.h>

// called this way, it uses the default address 0x40
Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();

#define MIN_PULSE_WIDTH       1000
#define MAX_PULSE_WIDTH       2100
#define DEFAULT_PULSE_WIDTH   1500
#define FREQUENCY             50

bool stringComplete = false;
char inData[100];
int dataIdx = 0;

void setup() {
  // OE is connected to pin 8.
  pinMode(8, OUTPUT);
  digitalWrite(8, LOW);
  
  Serial.begin(38400);
  Serial.println("16 channel Servo test!");

  pwm.begin();
  pwm.setPWMFreq(FREQUENCY);  // Analog servos run at ~60 Hz updates
}

void loop() {  
  if(stringComplete)
  {
    if(inData[0] == 't' && inData[2] == 'p')
    {
      int servoNum = ctoi(inData[1]);
      int servoPos = ctoi(inData[3]) * 100 + ctoi(inData[4]) * 10 + ctoi(inData[5]);
      if(servoNum == 0)
      {
        if(0 <= servoPos && servoPos <= 180)
        {
          setAllPosition(servoPos);         
        }
        else
        {
          turnAllOff();
        }
      }
      else if(servoNum <= 8 && servoNum > 0)
      {
        if(0 <= servoPos && servoPos <= 180)
        {
          setPosition(servoNum + 7, servoPos);         
        }
        else
        {
          turnOff(servoNum + 7);
        }
      }
    }
    stringComplete = false;
  }
}

void serialEvent()
{
  while(Serial.available() && stringComplete == false)
  {
    char inChar = Serial.read();
    inData[dataIdx++] = inChar;

    if(inChar == '\n')
    {
      dataIdx = 0;
      stringComplete = true;
    }
  }
}

int ctoi(char c)
{
  return c - '0';
}

void setAllPosition(int pos)
{
  for(int i = 0; i < 16; i++)
    setPosition(i, pos);
}

void turnAllOff()
{
  for(int i = 0; i < 16; i++)
    turnOff(i);
}

// i = 0 ... 15, servo #
// pos = 0 ... 180, servo position
void setPosition(int i, int pos)
{
  pwm.setPWM(i, 0, pulseWidth(pos));
  Serial.print("tactor");
  Serial.print(i);
  Serial.print(": ");
  Serial.println(pos);
}

// i = 0 ... 15, servo #
void turnOff(int i)
{
  pwm.setPWM(i, 0, 0);
  Serial.print("tactor");
  Serial.print(i);
  Serial.println(": off");
}

//GH: What is "analog_value"? Duty cicle? Check the datasheet.
int pulseWidth(int angle)
{
  int pulse_wide, analog_value;
  pulse_wide   = map(angle, 0, 180, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH);
  analog_value = int(float(pulse_wide) / 1000000 * FREQUENCY * 4096);
  return analog_value;
}
