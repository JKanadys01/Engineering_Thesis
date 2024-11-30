using SharpGL;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Speech.Synthesis;
using System.Text;

namespace GestureDetectionApp
{
    public partial class Form1 : Form
    {

        private Quaternion orientation = Quaternion.Identity;
        private Quaternion gyroQuaternion;
        
        private List<UdpClient> udpClients = new List<UdpClient>();
        private Dictionary<UdpClient, int> clientPorts = new Dictionary<UdpClient, int>();
        private const int port1 = 12345;
        private const int port2 = 12346;
        private const int port3 = 12347;

        private bool isSpeech = true;

        float width = 1.0f;
        float height = 2.0f;
        float depth = .5f;

        SpeechSynthesizer synthesizer;
        public Form1()
        {
            InitializeComponent();
            this.Text = "Gesture Detection";
            // Tworzenie klientów UDP i ich dodanie do listy
            AddUdpClient(port1);
            AddUdpClient(port2);
            AddUdpClient(port3);
            synthesizer = new SpeechSynthesizer();

            openglControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
        }

        private void AddUdpClient(int port)
        {
            var udpClient = new UdpClient(port);
            udpClients.Add(udpClient);
            clientPorts[udpClient] = port;
            BeginReceive(udpClient);
        }

        private void BeginReceive(UdpClient udpClient)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient.BeginReceive(ar => ReceiveCallback(ar, udpClient), remoteEndPoint);
        }

        private void ReceiveCallback(IAsyncResult ar, UdpClient udpClient)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes);

            // Rozró¿nienie klienta na podstawie portu
            if (clientPorts.TryGetValue(udpClient, out int port))
            {
                switch (port)
                {
                    case port1:
                        ProcessGestureData(receivedData);
                        break;
                    case port2:
                        ProcessLogData(receivedData);
                        break;
                    case port3:
                        ProcessVisualizationData(receivedData);
                        break;
                }
            }
            // Restart nas³uchiwania
            BeginReceive(udpClient);
        }

        private void ProcessGestureData(string data)
        {
            if (isSpeech)
            {
                UpdateTextBoxGesture(data);
            }
        }

        private void ProcessLogData(string data)
        {
            UpdateTextBoxLog(data);
        }

        private void ProcessVisualizationData(string data)
        {
            var values = data.Split(',');
            if (values.Length == 7)
            {
                gyroQuaternion = new Quaternion(
                    float.Parse(values[1], CultureInfo.InvariantCulture),
                    float.Parse(values[2], CultureInfo.InvariantCulture),
                    float.Parse(values[3], CultureInfo.InvariantCulture),
                    float.Parse(values[0], CultureInfo.InvariantCulture)
                );
                orientation = gyroQuaternion;

                openglControl.Invalidate();
            }
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {

            OpenGL gl = this.openglControl.OpenGL;

            // Clear the scene
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(0, 0, -5);


            DrawObjects(gl);


            gl.Flush();
        }

        private void DrawObjects(OpenGL gl)
        {
            float[] rotationMatrix = new float[16];
            Matrix4x4 rotationMat = Matrix4x4.CreateFromQuaternion(orientation);
            CopyMatrixToOpenGL(rotationMat, rotationMatrix);
            gl.MultMatrix(rotationMatrix);

            // Rysowanie prostopad³oœcianu
            gl.Begin(OpenGL.GL_QUADS);


            gl.Color(1.0f, 0.0f, 0.0f); // Czerwony kolor
            gl.Vertex(-width / 2, -height / 2, depth / 2);
            gl.Vertex(width / 2, -height / 2, depth / 2);
            gl.Vertex(width / 2, height / 2, depth / 2);
            gl.Vertex(-width / 2, height / 2, depth / 2);


            gl.Color(0.0f, 1.0f, 0.0f); // Zielony kolor
            gl.Vertex(-width / 2, -height / 2, -depth / 2);
            gl.Vertex(-width / 2, height / 2, -depth / 2);
            gl.Vertex(width / 2, height / 2, -depth / 2);
            gl.Vertex(width / 2, -height / 2, -depth / 2);


            gl.Color(0.0f, 0.0f, 1.0f); // Niebieski kolor
            gl.Vertex(-width / 2, -height / 2, -depth / 2);
            gl.Vertex(-width / 2, -height / 2, depth / 2);
            gl.Vertex(-width / 2, height / 2, depth / 2);
            gl.Vertex(-width / 2, height / 2, -depth / 2);


            gl.Color(1.0f, 1.0f, 0.0f); // ¯ó³ty kolor
            gl.Vertex(width / 2, -height / 2, -depth / 2);
            gl.Vertex(width / 2, height / 2, -depth / 2);
            gl.Vertex(width / 2, height / 2, depth / 2);
            gl.Vertex(width / 2, -height / 2, depth / 2);


            gl.Color(1.0f, 0.0f, 1.0f); // Magenta
            gl.Vertex(-width / 2, height / 2, -depth / 2);
            gl.Vertex(-width / 2, height / 2, depth / 2);
            gl.Vertex(width / 2, height / 2, depth / 2);
            gl.Vertex(width / 2, height / 2, -depth / 2);


            gl.Color(0.0f, 1.0f, 1.0f); // Cyjan
            gl.Vertex(-width / 2, -height / 2, -depth / 2);
            gl.Vertex(width / 2, -height / 2, -depth / 2);
            gl.Vertex(width / 2, -height / 2, depth / 2);
            gl.Vertex(-width / 2, -height / 2, depth / 2);

            gl.End();
        }

        private void CopyMatrixToOpenGL(Matrix4x4 matrix, float[] openGLMatrix)
        {
            // OpenGL u¿ywa kolejnoœci kolumnowej, podczas gdy C# u¿ywa kolejnoœci wierszowej
            openGLMatrix[0] = matrix.M11;
            openGLMatrix[1] = matrix.M12;
            openGLMatrix[2] = matrix.M13;
            openGLMatrix[3] = matrix.M14;

            openGLMatrix[4] = matrix.M21;
            openGLMatrix[5] = matrix.M22;
            openGLMatrix[6] = matrix.M23;
            openGLMatrix[7] = matrix.M24;

            openGLMatrix[8] = matrix.M31;
            openGLMatrix[9] = matrix.M32;
            openGLMatrix[10] = matrix.M33;
            openGLMatrix[11] = matrix.M34;

            openGLMatrix[12] = matrix.M41;
            openGLMatrix[13] = matrix.M42;
            openGLMatrix[14] = matrix.M43;
            openGLMatrix[15] = matrix.M44;
        }

        private void UpdateTextBoxGesture(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateTextBoxGesture(text)));
            }
            else
            {
                textBox_gesture.Text = text;
                SpeekText(text);
            }
        }

        private void UpdateTextBoxLog(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateTextBoxLog(text)));
            }
            else
            {
                textBox_log.Text += text + Environment.NewLine;
                // Ustawienie kursora na koñcu tekstu i przewiniêcie
                textBox_log.SelectionStart = textBox_log.Text.Length;
                textBox_log.ScrollToCaret();
            }
        }

        private void SpeekText(string text)
        {
            synthesizer.SpeakAsync(text);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            foreach (var udpClient in udpClients)
            {
                udpClient.Close();
            }
            synthesizer.Dispose();
        }

        private void button_log_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt";
            saveFileDialog.Title = "Save Log File";
            saveFileDialog.FileName = "log.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, textBox_log.Text);
                MessageBox.Show("Log saved successfully!", "Save Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button_speech_Click(object sender, EventArgs e)
        {
            isSpeech = !isSpeech;
        }
    }
}
