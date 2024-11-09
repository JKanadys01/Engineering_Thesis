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
        private Vector3 accValue = Vector3.Zero;  // Wartoœci przyspieszenia
        private UdpClient udpClient3;
        private const int port = 12347;
        private bool isSpeach = true;


        float width = 1.0f;
        float height = 2.0f;
        float depth = .5f;

        UdpClient udpClient;
        UdpClient udpClient2;
        SpeechSynthesizer synthesizer;
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient(12345);
            udpClient2 = new UdpClient(12346);
            udpClient.BeginReceive(ReceiveCallback, null);
            udpClient2.BeginReceive(ReceiveLog, null);
            synthesizer = new SpeechSynthesizer();

            openglControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
            udpClient3 = new UdpClient(port);
            BeginReceive();
        }

        private void BeginReceive()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient3.BeginReceive(new AsyncCallback(ReceiveCallbacV), remoteEndPoint);
        }

        private void ReceiveCallbacV(IAsyncResult ar)
        {
            IPEndPoint remoteEndPoint = (IPEndPoint)ar.AsyncState;
            byte[] receivedBytes = udpClient3.EndReceive(ar, ref remoteEndPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            ProcessReceivedData(receivedData);
            BeginReceive();
        }

        private void ProcessReceivedData(string data)
        {
            var values = data.Split(',');
            if (values.Length == 7)
            {
                gyroQuaternion = new Quaternion(
                    float.Parse(values[1], CultureInfo.InvariantCulture),
                    float.Parse(values[2], CultureInfo.InvariantCulture),
                    float.Parse(values[3], CultureInfo.InvariantCulture),
                    float.Parse(values[0], CultureInfo.InvariantCulture) // Wartoœæ `w` na koñcu
                );
                accValue[0] += float.Parse(values[4], CultureInfo.InvariantCulture) * 0.01f;
                accValue[1] += float.Parse(values[5], CultureInfo.InvariantCulture) * 0.01f;
                accValue[2] += float.Parse(values[6], CultureInfo.InvariantCulture) * 0.01f;
                orientation = gyroQuaternion;

                // przerysuj symulacjê
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
            // OpenGL uses column-major order, while C# uses row-major order.
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

        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12345);
            byte[] bytes = udpClient.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            if (isSpeach)
            {
                UpdateTextBoxGesture(message);
            }
            udpClient.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveLog(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12346);
            byte[] bytes = udpClient2.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            UpdateTextBoxLog(message);
            udpClient2.BeginReceive(ReceiveLog, null);
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
                SpeakText(text);
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

        private void SpeakText(string text)
        {
            synthesizer.SpeakAsync(text);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            udpClient.Close();
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
            isSpeach = !isSpeach;
        }
    }
}
