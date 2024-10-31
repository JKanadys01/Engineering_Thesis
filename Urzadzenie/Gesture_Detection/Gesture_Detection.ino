#include <WiFi.h>
#include <WiFiUdp.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <Adafruit_HMC5883_U.h>
Adafruit_MPU6050 mpu;
Adafruit_HMC5883_Unified magnetometer = Adafruit_HMC5883_Unified(123458);
const char *pcIP = "192.168.38.34";
const char *ssid2 = "Korbank_2.4G_45C23C";
const char *password2 = "79200948";
const char *ssid = "Redmi Note 12";
const char *password = "kubus123";
const int port = 12345;
const int logport = 12346;
const int simulationport = 12347;

WiFiUDP udp;

const float GYRO_THRESHOLD = 4.0;
const float ACC_THRESHOLD = 5.0;
const int GESTURE_TIME = 1000; // Czas oczekiwania między kolejnymi detekcjami.

float initialAcc[3] = {0, 0, 0};
float deltaAcc[3] = {0, 0, 0};
float gyroValues[3] = {0, 0, 0};
float accValues[3] = {0, 0, 0};
float magValues[3] = {0, 0, 0};
const char* accGestures[3][2] = {
    {"Acceleration Right", "Acceleration Left"},
    {"Acceleration Forward", "Acceleration Backward"},
    {"Acceleration Upward", "Acceleration Downward"}
};

const char* gyroGestures[3][2] = {
    {"Rotating Upward", "Rotating Downward"},
    {"Rotating Forward", "Rotating Backward"},
    {"Rotating to the Left", "Rotating to the Right"}
};

unsigned long lastGestureTime = 0; // Czas ostatniego wykrytego gestu

void setup() {
  Serial.begin(115200);
  while (!Serial);

  Serial.println("Adafruit MPU6050 Gesture Detection");

  // Inicjalizacja MPU6050
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) delay(10);
  }
  Serial.println("MPU6050 Found!");
  // Konfiguracja czujników
  mpu.setAccelerometerRange(MPU6050_RANGE_4_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_10_HZ);

   /* Initialise the sensor */
  if(!magnetometer.begin())
  {
    /* There was a problem detecting the HMC5883 ... check your connections */
    Serial.println("Ooops, no HMC5883 detected ... Check your wiring!");
    while(1);
  }

  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");

  udp.begin(port);

  sensors_event_t accel, gyro, temp, mag;
  mpu.getEvent(&accel, &gyro, &temp);
  magnetometer.getEvent(&mag);
  // Ustawienie początkowych wartości akcelerometru
  initialAcc[0] = accel.acceleration.x;
  initialAcc[1] = accel.acceleration.y;
  initialAcc[2] = accel.acceleration.z;

  delay(100);
}

void loop() {
    // Odczyt danych z akcelerometru i żyroskopu
    sensors_event_t accel, gyro, temp, mag;
    mpu.getEvent(&accel, &gyro, &temp);
    magnetometer.getEvent(&mag);
    
    magValues[0] = mag.magnetic.x;
    magValues[1] = mag.magnetic.y;
    

    accValues[0] = accel.acceleration.x;
    accValues[1] = accel.acceleration.y;
    accValues[2] = accel.acceleration.z;

    gyroValues[0] = gyro.gyro.x;
    gyroValues[1] = gyro.gyro.y;
    gyroValues[2] = gyro.gyro.z;

    magValues[0] = mag.magnetic.x;
    magValues[1] = mag.magnetic.y;
    magValues[2] = mag.magnetic.z;

    sendSimulation(accValues,gyroValues,magValues);

    deltaAcc[0] = accel.acceleration.x - initialAcc[0];
    deltaAcc[1] = accel.acceleration.y - initialAcc[1];
    deltaAcc[2] = accel.acceleration.z - initialAcc[2];

    unsigned long currentTime = millis();

    // Sprawdzenie, czy minął wymagany czas od ostatniego wykrytego gestu
    if (currentTime - lastGestureTime >= GESTURE_TIME) {
      // Detekcja gestów akcelerometru
      for (int i = 0; i < 3; ++i) {
        if (deltaAcc[i] > ACC_THRESHOLD) {
          sendMotion(accGestures[i][0]);
          sendLog("Accel", deltaAcc, gyroValues);
          //Serial.println(accGestures[i][0]);
          lastGestureTime = currentTime;
          break;
        } else if (deltaAcc[i] < -ACC_THRESHOLD) {
          sendMotion(accGestures[i][1]);
          sendLog("Accel", deltaAcc, gyroValues);
          //Serial.println(accGestures[i][1]);
          lastGestureTime = currentTime;
          break;
        }
      }

      // Detekcja gestów żyroskopu
      
      for (int i = 0; i < 3; ++i) {
        if (gyroValues[i] > GYRO_THRESHOLD) {
          sendMotion(gyroGestures[i][0]);
          sendLog("Gyro", deltaAcc, gyroValues);
          //Serial.println(gyroGestures[i][0]);
          lastGestureTime = currentTime;
          break;
        } else if (gyroValues[i] < -GYRO_THRESHOLD) {
          sendMotion(gyroGestures[i][1]);
          sendLog("Gyro", deltaAcc, gyroValues);
          //Serial.println(gyroGestures[i][1]);
          lastGestureTime = currentTime;
          break;
        }
      }
    }

    /*
    Serial.print("AccelX:");
    Serial.print(accValues[0]);
    Serial.print(",");
    Serial.print("AccelY:");
    Serial.print(accValues[1]);
    Serial.print(",");
    Serial.print("AccelZ:");
    Serial.print(accValues[2]);
    Serial.print(", ");
    Serial.print("GyroX:");
    Serial.print(gyroValues[0]);
    Serial.print(",");
    Serial.print("GyroY:");
    Serial.print(gyroValues[1]);
    Serial.print(",");
    Serial.print("GyroZ:");
    Serial.print(gyroValues[2]);
    Serial.println("");
    
    */
    Serial.print("MagX:");
    Serial.print(magValues[0]);
    Serial.print(",");
    Serial.print("MagY:");
    Serial.print(magValues[1]);
    Serial.print(",");
    Serial.print("MagZ:");
    Serial.print(magValues[2]);
    Serial.println("");
    

  delay(10); // Częstotliwość próbkowania
}

void sendMotion(const char *motion) {
  udp.beginPacket(pcIP, port);
  udp.print(motion);
  udp.endPacket();
}

void sendLog(const char* type, float acc[3], float gyro[3]) {
  udp.beginPacket(pcIP, logport); 
  udp.print(type);
  udp.print("Accel: X=");
  udp.print(acc[0]);
  udp.print(", Y=");
  udp.print(acc[1]);
  udp.print(", Z=");
  udp.print(acc[2]);
  udp.print("; Gyro: X=");
  udp.print(gyro[0]);
  udp.print(", Y=");
  udp.print(gyro[1]);
  udp.print(", Z=");
  udp.print(gyro[2]);
  udp.endPacket();
}

void sendSimulation(float acc[3], float gyro[3],float mag[3]) {
  udp.beginPacket(pcIP, 12347); // Port do uproszczonych danych
  udp.print(acc[0]);
  udp.print(",");
  udp.print(acc[1]);
  udp.print(",");
  udp.print(acc[2]);
  udp.print(",");
  udp.print(gyro[0]);
  udp.print(",");
  udp.print(gyro[1]);
  udp.print(",");
  udp.print(gyro[2]);
  udp.print(",");
  udp.print(mag[0]);
  udp.print(",");
  udp.print(mag[1]);
  udp.print(",");
  udp.print(mag[2]);
  udp.endPacket();
}

