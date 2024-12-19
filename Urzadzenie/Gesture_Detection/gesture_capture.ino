#include <Wire.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <vector>
#include <math.h>

// Inicjalizacja MPU6050
Adafruit_MPU6050 mpu;

// Rozmiar próbki
#define SAMPLE_SIZE 50

// Struktura do przechowywania danych przyspieszenia i żyroskopu
struct SensorData {
  float ax, ay, az;  
  float gx, gy, gz;  
};

// Wektory do przechowywania danych z gestu
std::vector<SensorData> gestureModel;
std::vector<SensorData> gestureInput;

// Inicjalizacja MPU6050
void setupMPU() {
  if (!mpu.begin()) {
    Serial.println("Nie można znaleźć MPU6050");
    while (1);
  }
  mpu.setAccelerometerRange(MPU6050_RANGE_2_G);
  mpu.setGyroRange(MPU6050_RANGE_250_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);
}


void collectData(std::vector<SensorData>& dataVector) {
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  SensorData data;
  data.ax = a.acceleration.x;
  data.ay = a.acceleration.y;
  data.az = a.acceleration.z;
  data.gx = g.gyro.x;
  data.gy = g.gyro.y;
  data.gz = g.gyro.z;
  
  dataVector.push_back(data);
}

// Funkcja obliczająca odległość DTW między dwoma zestawami danych
float dtwDistance(const std::vector<SensorData>& model, const std::vector<SensorData>& input) {
  int n = model.size();
  int m = input.size();
  
  // Macierz odległości
  std::vector<std::vector<float>> dtw(n, std::vector<float>(m, INFINITY));
  
  dtw[0][0] = 0;
  
  for (int i = 1; i < n; i++) {
    dtw[i][0] = dtw[i-1][0] + euclideanDistance(model[i], input[0]);
  }
  
  for (int j = 1; j < m; j++) {
    dtw[0][j] = dtw[0][j-1] + euclideanDistance(model[0], input[j]);
  }

  for (int i = 1; i < n; i++) {
    for (int j = 1; j < m; j++) {
      float cost = euclideanDistance(model[i], input[j]);
      dtw[i][j] = cost + std::min({dtw[i-1][j], dtw[i][j-1], dtw[i-1][j-1]});
    }
  }

  return dtw[n-1][m-1];
}

// Funkcja obliczająca odległość euklidesową między dwoma punktami
float euclideanDistance(const SensorData& a, const SensorData& b) {
  return sqrt(pow(a.ax - b.ax, 2) + pow(a.ay - b.ay, 2) + pow(a.az - b.az, 2) +
              pow(a.gx - b.gx, 2) + pow(a.gy - b.gy, 2) + pow(a.gz - b.gz, 2));
}

void setup() {
  Serial.begin(115200);
  setupMPU();

  Serial.println("Zbieranie danych...");
  
  // Zbieranie danych modelu gestu
  for (int i = 0; i < SAMPLE_SIZE; i++) {
    collectData(gestureModel);
    delay(50);  // Mała przerwa między próbkami
  }

  Serial.println("Model gestu zebrany");
}

void loop() {
  // Zbieranie nowych danych do wykrywania gestu
  gestureInput.clear();
  Serial.println("Zbieranie danych wejściowych...");
  for (int i = 0; i < SAMPLE_SIZE; i++) {
    collectData(gestureInput);
    delay(50);  // Mała przerwa między próbkami
  }

  // Obliczanie odległości DTW
  float distance = dtwDistance(gestureModel, gestureInput);
  Serial.print("Odległość DTW: ");
  Serial.println(distance);

  // Przykład rozpoznawania gestu (np. jeśli odległość jest poniżej progu)
  if (distance < 60.0) {
    Serial.println("Gest rozpoznany!");
  } 

  delay(2000);  // Czekamy przed kolejnym pomiarem
}
