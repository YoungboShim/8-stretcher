#include <Wire.h>
#include <Adafruit_PWMServoDriver.h>

// called this way, it uses the default address 0x40
Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();

#define MIN_PULSE_WIDTH       1000
#define MAX_PULSE_WIDTH       2100
#define DEFAULT_PULSE_WIDTH   1500
#define FREQUENCY             50


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
  if(Serial.available())
  {
    String cmd = Serial.readString();
    cmd.trim();
    Serial.println(cmd);
    if(cmd[0] == 't' && cmd[2] == 'p' && cmd.length() == 6)
    {
      Serial.println("In the loop");
      int servoNum = cmd.substring(1,2).toInt();
      int servoPos = cmd.substring(3,6).toInt();
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
          Serial.print("tactor");
          Serial.print(servoNum);
          Serial.print(": ");
          Serial.println(servoPos);
        }
        else
        {
          turnOff(servoNum + 7);
          Serial.print("tactor");
          Serial.print(servoNum);
          Serial.println(": off");
        }
      }
      delay(500);
    }
  }
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
}

// i = 0 ... 15, servo #
void turnOff(int i)
{
  pwm.setPWM(i, 0, 0);
}

//GH: What is "analog_value"? Duty cicle? Check the datasheet.
int pulseWidth(int angle)
{
  int pulse_wide, analog_value;
  pulse_wide   = map(angle, 0, 180, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH);
  analog_value = int(float(pulse_wide) / 1000000 * FREQUENCY * 4096);
  Serial.println(analog_value);
  return analog_value;
}
