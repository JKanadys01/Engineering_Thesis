using System.Net;
using System.Net.Sockets;
using System.Speech.Synthesis;
using System.Text;

namespace GestureDetectionApp
{
    public partial class Form1 : Form
    {
        private SimulationForm simulationForm;
        private bool isSpeechEnabled = true;

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
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (isSpeechEnabled)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12345);
                byte[] bytes = udpClient.EndReceive(ar, ref ip);
                string message = Encoding.ASCII.GetString(bytes);
                UpdateTextBoxGesture(message);
                udpClient.BeginReceive(ReceiveCallback, null);
            }
        }

        private void ReceiveLog(IAsyncResult ar)
        {
            if (isSpeechEnabled)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12346);
                byte[] bytes = udpClient2.EndReceive(ar, ref ip);
                string message = Encoding.ASCII.GetString(bytes);
                UpdateTextBoxLog(message);
                udpClient2.BeginReceive(ReceiveLog, null);
            }
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
                // Only speak if speech is enabled
                if (isSpeechEnabled)
                {
                    SpeakText(text);
                }
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

        private void button_simulation_Click(object sender, EventArgs e)
        {
            isSpeechEnabled = false;
            if (simulationForm == null || simulationForm.IsDisposed)
            {
                simulationForm = new SimulationForm();
                simulationForm.Show();
            }
        }
    }
}
