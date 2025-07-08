#include <WiFi.h>
#include <WiFiUdp.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <Adafruit_HMC5883_U.h>

Adafruit_MPU6050 mpu;
Adafruit_HMC5883_Unified magnetometer = Adafruit_HMC5883_Unified(123458);
// Nazwa i hasło do sieci nr 1
const char *pcIP = "192.168.1.5";
const char *ssid = "Korbank";
const char *password = "792";
// Nazwa i hasło do sieci nr 2
const char *ssid2 = "Redmi 12";
const char *password2 = "plplpl";

// Numery portow dla Udp
const int port = 12345;
const int logport = 12346;
const int visualizationport = 12347;

// Bufor na wartości czujników
const int BUFFER_SIZE = 10;  // Liczba próbek do buforowania
float accBuffer[3][BUFFER_SIZE];  // Bufor dla akcelerometru (x, y, z)
float gyroBuffer[3][BUFFER_SIZE];  // Bufor dla żyroskopu (x, y, z)
int bufferIndex = 0;

WiFiUDP udp;

const float GYRO_THRESHOLD = 2.0;
const float ACC_THRESHOLD = 3.0;
const int GESTURE_TIME = 1000; // Czas oczekiwania między kolejnymi detekcjami.


float initialAcc[3] = {0, 0, 0};
float deltaAcc[3] = {0, 0, 0};
float gyroValues[3] = {0, 0, 0};
float accValues[3] = {0, 0, 0};
float magValues[3] = {0, 0, 0};

float orientation[4] = {1, 0, 0, 0}; // Kwaternion początkowy (w, x, y, z)
float dt = 0.016;
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
  Serial.println("Gesture Detection");
  // Inicjalizacja MPU6050
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050");
    while (1) delay(10);
  }
  // Konfiguracja czujników
  mpu.setAccelerometerRange(MPU6050_RANGE_16_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_10_HZ);

  if(!magnetometer.begin())
  {
    Serial.println("Failed to find HMC5883");
    while(1) delay(10);
  }
  magnetometer.setMagGain(HMC5883_MAGGAIN_1_3); // Ustawia przyrost na poziomie 1.3 Gaussa

  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  sensors_event_t accel, gyro, temp;
  mpu.getEvent(&accel, &gyro, &temp);
  // Ustawienie początkowych wartości akcelerometru
  initialAcc[0] = accel.acceleration.x;
  initialAcc[1] = accel.acceleration.y;
  initialAcc[2] = accel.acceleration.z;

  delay(100);
}

void loop() 
{
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

    deltaAcc[0] = accel.acceleration.x - initialAcc[0];
    deltaAcc[1] = accel.acceleration.y - initialAcc[1];
    deltaAcc[2] = accel.acceleration.z - initialAcc[2];

    // Przetworzenie kwaternionu z danych żyroskopu
    processQuaternion(orientation, gyroValues,accValues,magValues, dt);

    // Wysyłanie zaktualizowanego kwaternionu przez UDP
    sendVisualization();

    // Aktualizacja bufora
    updateBuffer(deltaAcc, gyroValues);

    unsigned long currentTime = millis();
      
    // Sprawdzenie, czy minął wymagany czas od ostatniego wykrytego gestu
    if (currentTime - lastGestureTime >= GESTURE_TIME) 
    {
      for (int axis = 0; axis < 3; ++axis) 
      {
        if(abs(accValues[2]) > (abs(accValues[0]) + abs(accValues[1])))
        {
          // Sprawdzenie gestów na podstawie akcelerometru
          if (isGestureDetected(axis, ACC_THRESHOLD, accBuffer)) 
          {
            sendMotion(accGestures[axis][(deltaAcc[axis] > 0) ? 0 : 1]);
            DataPrint();
            sendLog("Accel", deltaAcc, gyroValues);
            lastGestureTime = currentTime;
            break;
          }
        }
        // Sprawdzenie gestów na podstawie żyroskopu
        if (isGestureDetected(axis, GYRO_THRESHOLD, gyroBuffer)) 
        {
          sendMotion(gyroGestures[axis][(gyroValues[axis] > 0) ? 0 : 1]);
          DataPrint();
          sendLog("Gyro", deltaAcc, gyroValues);
          lastGestureTime = currentTime;
          break;
        }
      }
    }
  delay(10);
}

void DataPrint()
{
  Serial.print("accX:");
    Serial.println(deltaAcc[0]);
    
    Serial.print("accY:");
    Serial.println(deltaAcc[1]);
    
    Serial.print("accZ:");
    Serial.println(deltaAcc[2]);

    Serial.print("gyroX:");
    Serial.println(gyroValues[0]);
    
    Serial.print("gyroY:");
    Serial.println(gyroValues[1]);
    
    Serial.print("gyroZ:");
    Serial.println(gyroValues[2]);
    Serial.print("..............\n");
}

float calculateAverage(float buffer[BUFFER_SIZE]) {
    float sum = 0;
    for (int i = 0; i < BUFFER_SIZE; i++) {
        sum += buffer[i];
    }
    return sum / BUFFER_SIZE;
}

// Funkcja do dodawania wartości do bufora
void updateBuffer(float acc[3], float gyro[3]) 
{
  for (int i = 0; i < 3; ++i) 
  {
    accBuffer[i][bufferIndex] = acc[i];
    gyroBuffer[i][bufferIndex] = gyro[i];
  }
  bufferIndex = (bufferIndex + 1) % BUFFER_SIZE;  
}

// Funkcja sprawdzająca, czy wartości w buforze przekraczają próg przez całą długość bufora
bool isGestureDetected(int axis, float threshold, float buffer[3][BUFFER_SIZE]) 
{
  float avgValue = calculateAverage(buffer[axis]);
  return abs(avgValue) > threshold;
}


void sendMotion(const char *motion) 
{
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

void sendVisualization() 
{
  // Wysyłanie kwaternionu po UDP
    char buffer[80];
    sprintf(buffer, "%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f", orientation[0], orientation[1], orientation[2], orientation[3],deltaAcc[0],deltaAcc[1],deltaAcc[2]);
    udp.beginPacket(pcIP, visualizationport);
    udp.print(buffer);
    udp.endPacket();
}


void processQuaternion(float orientation[4], float gyroValues[3], float accValues[3], float magValues[3], float dt) 
{
  // Zamiana wartości z żyroskopu na kwaternion rotacji
    float halfDeltaX = gyroValues[0] * 0.5f * dt;
    float halfDeltaY = gyroValues[1] * 0.5f * dt;
    float halfDeltaZ = gyroValues[2] * 0.5f * dt;

    // Obliczenie przyrostu kwaternionu z żyroskopu
    float deltaQuat[4] = {
        cos(sqrt(halfDeltaX * halfDeltaX + halfDeltaY * halfDeltaY + halfDeltaZ * halfDeltaZ)),
        sin(halfDeltaX),
        sin(halfDeltaY),
        sin(halfDeltaZ)
    };

    // Mnożenie kwaternionu orientacji przez przyrost kwaternionu
    float q1 = orientation[0];
    float q2 = orientation[1];
    float q3 = orientation[2];
    float q4 = orientation[3];

    orientation[0] = q1 * deltaQuat[0] - q2 * deltaQuat[1] - q3 * deltaQuat[2] - q4 * deltaQuat[3];
    orientation[1] = q1 * deltaQuat[1] + q2 * deltaQuat[0] + q3 * deltaQuat[3] - q4 * deltaQuat[2];
    orientation[2] = q1 * deltaQuat[2] - q2 * deltaQuat[3] + q3 * deltaQuat[0] + q4 * deltaQuat[1];
    orientation[3] = q1 * deltaQuat[3] + q2 * deltaQuat[2] - q3 * deltaQuat[1] + q4 * deltaQuat[0];

    // Normalizacja kwaternionu
    float norm = sqrt(orientation[0] * orientation[0] + orientation[1] * orientation[1] +
                      orientation[2] * orientation[2] + orientation[3] * orientation[3]);
    orientation[0] /= norm;
    orientation[1] /= norm;
    orientation[2] /= norm;
    orientation[3] /= norm;

    // Obliczanie kątów z akcelerometru
    float roll = atan2(-accValues[0], sqrt(accValues[1] * accValues[1] + accValues[2] * accValues[2]));
    float pitch = atan2(accValues[1], sqrt(accValues[0] * accValues[0] + accValues[2] * accValues[2]));
    float yaw = atan2(2.0f * (orientation[0] * orientation[3] + orientation[1] * orientation[2]),
                1.0f - 2.0f * (orientation[2] * orientation[2] + orientation[3] * orientation[3]));
    //Oś Z do góry
    if (accValues[2] > 9)
    {
      yaw = atan2(magValues[0], magValues[1]);
      yaw += 1.2f;
    }
    // Oś Y do góry
    if (accValues[1] > 7)
    {
        yaw = atan2(magValues[1], magValues[2]);
    }
    // Oś Y do dołu
    if (accValues[1] < -7)
    {
        yaw = atan2(magValues[1], -magValues[2]);
    }
    //Oś X do góry
    if (accValues[0] > 6)
    {
      yaw = atan2(magValues[2], -magValues[0]);
    }
    //Oś X do dołu
    if (accValues[0] < -6)
    {
      yaw = atan2(-magValues[2], -magValues[0]);
    }

    float accQuat[4];
    accQuat[0] = cos(pitch * 0.5f) * cos(roll * 0.5f) * cos(yaw * 0.5f) + sin(pitch * 0.5f) * sin(roll * 0.5f) * sin(yaw * 0.5f);
    accQuat[1] = sin(pitch * 0.5f) * cos(roll * 0.5f) * cos(yaw * 0.5f) - cos(pitch * 0.5f) * sin(roll * 0.5f) * sin(yaw * 0.5f);
    accQuat[2] = cos(pitch * 0.5f) * sin(roll * 0.5f) * cos(yaw * 0.5f) + sin(pitch * 0.5f) * cos(roll * 0.5f) * sin(yaw * 0.5f);
    accQuat[3] = cos(pitch * 0.5f) * cos(roll * 0.5f) * sin(yaw * 0.5f) - sin(pitch * 0.5f) * sin(roll * 0.5f) * cos(yaw * 0.5f);

    // Normalizacja kwaternionu akcelerometru
    float accQuatNorm = sqrt(accQuat[0] * accQuat[0] + accQuat[1] * accQuat[1] + accQuat[2] * accQuat[2] + accQuat[3] * accQuat[3]);
    for (int i = 0; i < 4; ++i) 
    {
        accQuat[i] /= accQuatNorm;
    }

    for (int i = 0; i < 4; ++i) 
    {
        orientation[i] = orientation[i] * 0.99 + accQuat[i] * 0.01;
    }

    // Normalizacja zaktualizowanego kwaternionu
    norm = sqrt(orientation[0] * orientation[0] + orientation[1] * orientation[1] +
                orientation[2] * orientation[2] + orientation[3] * orientation[3]);
    orientation[0] /= norm;
    orientation[1] /= norm;
    orientation[2] /= norm;
    orientation[3] /= norm;
}
