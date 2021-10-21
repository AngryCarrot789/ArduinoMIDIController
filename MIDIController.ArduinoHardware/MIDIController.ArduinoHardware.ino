#define AREAD_TO_MIDI 0.125f

void setup() {
	Serial.begin(9600);
}

void loop() {
	Serial.write(analogRead(A2) / 8);
	Serial.write(analogRead(A3) / 8);
	Serial.write(analogRead(A4) / 8);
	Serial.write(analogRead(A5) / 8);
	Serial.write(129);

	delay(100);
}