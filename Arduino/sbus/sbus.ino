/*
This is a very cursed file. Because I lacked a UART I ended up using an Arduino Uno, which is how this travesty came to be.

You need a logic level inverter connected like this:
Arduino RX -> NPN Collector
Sbus port -> 10k resistor -> NPN Base
Ground -> NPN Emitter
*/

bool oldState = false;
long lastTime = 0;
byte buffer[25];
int writePos = 0;
bool syncronised = false;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, HIGH);
  Serial.begin(100000, SERIAL_8E2);
}

void loop() {
  long currentTime = millis();
  while (Serial.available())
  {
    byte inData = Serial.read();
    buffer[writePos] = inData;
    if (!syncronised)
    {
      if (writePos == 24 && buffer[24] == 0x00)
      {
        writePos = 0;
        continue;
      }
      if (writePos == 0 && buffer[0] == 0x0F)
      {
        syncronised = true;
        writePos = 1;
      }
      else
      {
          writePos = 24;
      }
    }

    if (!syncronised)
    {
      continue;
    }

    if (syncronised)
    {
      writePos++;
    }
    
    if (writePos == 25)
    {
      if (buffer[0] != 0x0F || buffer[24] != 0x00)
      {
        writePos = 24;
        syncronised = false;
      }
      lastTime = currentTime;
      writePos = 0;
      Serial.end();
      Serial.begin(115200, SERIAL_8N1);
      Serial.write(buffer, 25);
      Serial.flush();
      Serial.end();
      Serial.begin(100000, SERIAL_8E2);
    }
  }
  bool newState = currentTime - lastTime < 5;
  if (newState != oldState)
  {
    oldState = newState;
    if (newState)
    {
      digitalWrite(LED_BUILTIN, LOW);
    }
    else
    {
      digitalWrite(LED_BUILTIN, HIGH);
    }
  }
}
