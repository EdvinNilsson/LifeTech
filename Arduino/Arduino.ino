#include <Wire.h>

void setup() {
	Wire.begin(8);
	Wire.onReceive(receiveEvent);
	Wire.onRequest(sendData);
	Serial.begin(9600);
}

void loop() {
	delay(100);
}

void receiveEvent(int howMany) {
	//byte command = Wire.read();
	Serial.println(howMany);
	while (Wire.available()) {
		byte b = Wire.read();
		Serial.println(b);
	}
}

void sendData() {
	byte bytes[] = { 1, 2, 3, 4 };
	Wire.write(bytes, 4);
}
