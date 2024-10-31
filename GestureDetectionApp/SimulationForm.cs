using SharpGL;
using SharpGL.SceneGraph.Lighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace GestureDetectionApp
{
    public partial class SimulationForm : Form
    {
        private Vector3 accValues = new Vector3(0, 0, 0);
        private Vector3 gyroValues = new Vector3(0, 0, 0);
        private Vector3 magValues = new Vector3(0,0,0);

        
        private Quaternion orientation = Quaternion.Identity;

        private UdpClient udpClient;
        private const int port = 12347;

        private const float alpha = 1.0f; 
        private const float dt = 0.016f; 

        float width = 1.0f;  
        float height = 2.0f; 
        float depth = .5f;  
        public SimulationForm()
        {
            InitializeComponent();
            openglControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
            udpClient = new UdpClient(port);
            BeginReceive();
        }

        private void BeginReceive()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), remoteEndPoint);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint remoteEndPoint = (IPEndPoint)ar.AsyncState;
            byte[] receivedBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            ProcessReceivedData(receivedData);
            BeginReceive();
        }

        private void ProcessReceivedData(string data)
        {
            var values = data.Split(',');
            if (values.Length == 9)
            {
                accValues.X = float.Parse(values[0], CultureInfo.InvariantCulture);
                accValues.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
                accValues.Z = float.Parse(values[2], CultureInfo.InvariantCulture);

                gyroValues.X = float.Parse(values[3], CultureInfo.InvariantCulture);
                gyroValues.Y = float.Parse(values[4], CultureInfo.InvariantCulture);
                gyroValues.Z = float.Parse(values[5], CultureInfo.InvariantCulture);

                magValues.X = float.Parse(values[6], CultureInfo.InvariantCulture);
                magValues.Y = float.Parse(values[7], CultureInfo.InvariantCulture);
                magValues.Z = float.Parse(values[8], CultureInfo.InvariantCulture);
                
                UpdateSimulation(accValues, gyroValues,magValues);
            }
        }

        private void UpdateSimulation(Vector3 acc, Vector3 gyro, Vector3 mag)
        {
            
            Vector3 gyroRad = new Vector3(gyro.X, gyro.Y, gyro.Z);

            
            Quaternion gyroQuat = Quaternion.CreateFromYawPitchRoll(gyroRad.Y * dt, gyroRad.X * dt, gyroRad.Z * dt);
            orientation = Quaternion.Multiply(orientation, gyroQuat);
            orientation = Quaternion.Normalize(orientation);
            
            Vector3 accAdjust = new Vector3(acc.X, acc.Y, acc.Z);

            Vector3 gravity = Vector3.Normalize(acc);

            float pitch = (float)Math.Atan2(-gravity.X, Math.Sqrt(gravity.Y * gravity.Y + gravity.Z * gravity.Z));
            float roll = (float)Math.Atan2(gravity.Y, Math.Sqrt(gravity.X * gravity.X + gravity.Z * gravity.Z));
            float yaw;




            // Wybór formuły dla yaw w zależności od osi skierowanej do góry
            if (Math.Abs(gravity.Z) > Math.Abs(gravity.X) && Math.Abs(gravity.Z) > Math.Abs(gravity.Y))
            {
                // Z-axis up or down
                if (mag.X > 0)
                {
                    yaw = (float)Math.Atan2(mag.Y, mag.X);
                }
                else
                {
                    yaw = (float)Math.Atan2(mag.Y, -mag.X);
                }
            }
            else if (Math.Abs(gravity.Y) > Math.Abs(gravity.X) && Math.Abs(gravity.Y) > Math.Abs(gravity.Z))
            {
                // Y-axis up or down
                if (mag.X > 0)
                {
                    yaw = (float)Math.Atan2(mag.Z, mag.X);
                }
                else
                {
                    yaw = (float)Math.Atan2(mag.Z, -mag.X);
                }
            }
            else
            {
                // X-axis up or down
                if (mag.Z > 0)
                {
                    yaw = (float)Math.Atan2(mag.Y, mag.Z);
                }
                else
                {
                    yaw = (float)Math.Atan2(mag.Y, -mag.Z);
                }
            }

            yaw += 0.9f;

            Quaternion accQuat = Quaternion.CreateFromYawPitchRoll(pitch, roll, yaw);

            // scalenie
            orientation = Quaternion.Slerp(orientation, accQuat, 1 - 0.99f);




            // przerysuj symulację
            openglControl.Invalidate();
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

            // Rysowanie prostopadłościanu
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

            
            gl.Color(1.0f, 1.0f, 0.0f); // Żółty kolor
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
    }
}
