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

void loop1() {
  setAllPosition(0);
  delay(500);
  turnAllOff();
  delay(1000);
  setAllPosition(180);
  delay(500);
  turnAllOff();
  delay(1000);
}

void loop() {
  setAllPosition(0);
  delay(500);
  turnAllOff();
  delay(1000);

  for(int i = 8; i < 16; i+=2){
    setPosition(i, 180);
    delay(300);
    turnOff(i);
  }
  for(int i = 9; i < 16; i+=2){
    setPosition(i, 180);
    delay(300);
    turnOff(i);
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
