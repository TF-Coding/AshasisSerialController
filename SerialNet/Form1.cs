using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace SerialNet
{
    public partial class Form1 : Form
    {
        String autoloadfile;
        public Form1(String autoload = null)
        {
            InitializeComponent();
            this.autoloadfile = autoload;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            Int32[] bauds = new Int32[] { 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 56000, 57600, 115200 };
            comboBox2.Text = "115200";
            foreach (int b in bauds)
            {
                comboBox2.Items.Add(b.ToString());
            }

            if (comboBox1.Text != "")
            {
                button1_Click_1(sender, e);
            }

            if (this.autoloadfile != null)
            {
                checkBox1.Checked = true;
                checkBox1_CheckedChanged(null, null);
                loadFile(this.autoloadfile);

                button1_Click_1(null, null);
            }
        }

        private void singleLineGot(String line)
        {
            byte[] buff = ASCIIEncoding.ASCII.GetBytes(line + "\n");
            clients.ForEach((c) =>
            {
                try
                {
                    c.GetStream().Write(buff, 0, buff.Length);
                }
                catch (Exception)
                {
                }

            });
            Console.WriteLine("sent: " + line);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (button1.Tag.ToString() == "start")
            {
                if (useSimulator) start_sim();
                else start();
            }
            else stop();
        }

        SerialPort sp;
        TcpListener listener;
        Thread tcpAccepter;
        List<TcpClient> clients = new List<TcpClient>();
        Thread spRead;

        long simDataMax = 0;
        long simDataLoop = -1;
        private void start_sim()
        {
            if (simulatorData.Count == 0)
            {
                status.Text = "Keine Simulationsdaten geladen";
                return;
            }
            int port;
            if (!int.TryParse(textBox1.Text, out port))
            {
                status.Text = "Ungültiger Port";
                return;
            }
            listener = new TcpListener(IPAddress.Any, port);
            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                status.Text = "Listener kann nicht gestartet werden - Port belegt?";
                return;
            }
            spRead = new Thread(new ThreadStart(() =>
            {
                List<int> markAsCalledIndices = new List<int>();

                simulatorData.ForEach((sd) =>
                {
                    simDataMax = (sd.delay > simDataMax ? sd.delay : simDataMax);
                });
                simDataLoop = -1;
                while (true)
                {

                    try
                    {
                        foreach (SimData sd in simulatorData)
                        {
                            if (sd.delay == simDataLoop)
                            {
                                if (sd.once && !markAsCalledIndices.Contains(simulatorData.IndexOf(sd)))
                                {
                                    singleLineGot(sd.data);
                                    selectListViewIndex(simulatorData.IndexOf(sd));
                                    markAsCalledIndices.Add(simulatorData.IndexOf(sd));
                                }
                                else if (!sd.once)
                                {
                                    singleLineGot(sd.data);
                                    selectListViewIndex(simulatorData.IndexOf(sd));
                                }
                            };
                        }
                        Thread.Sleep(1000);
                        simDataLoop++;
                        if (simDataLoop > simDataMax && true) simDataLoop = 0;
                        //if (simDataLoop > simDataMax && !checkBox2.Checked) stop();

                    }
                    catch (ThreadInterruptedException) { }
                    catch (ThreadAbortException) { }
                }
            }));
            tcpAccepter = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        while (true)
                        {
                            TcpClient c = listener.AcceptTcpClient();
                            clients.Add(c);
                        }
                    }
                    catch (ThreadInterruptedException) { }
                    catch (ThreadAbortException) { }
                }));
            spRead.Start();
            tcpAccepter.Start();

            status.Text = "Läuft";
            button1.Tag = "stop";
            button1.Text = "Stop";
        }

        private void start()
        {

            if (!SerialPort.GetPortNames().Contains(comboBox1.Text))
            {
                status.Text = "SerialPort unbekannt";
                return;
            }

            int baudrate;
            if (!int.TryParse(comboBox2.Text, out baudrate))
            {
                status.Text = "Ungültige Baudrate";
                return;
            }

            sp = new SerialPort(comboBox1.Text, baudrate);
            try
            {
                sp.Open();
            }
            catch (Exception e)
            {
                status.Text = "Portfehler: " + e.Message;
                return;
            }
            int port;
            if (!int.TryParse(textBox1.Text, out port))
            {
                status.Text = "Ungültiger Port";
                return;
            }
            listener = new TcpListener(IPAddress.Any, port);
            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                status.Text = "Listener kann nicht gestartet werden - Port belegt?";
                return;
            }

            status.Text = "Öffne...";
            Thread.Sleep(500);
            if (sp.IsOpen)
            {
                spRead = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        while (true)
                        {
                            singleLineGot(sp.ReadLine());
                        }
                    }
                    catch (ThreadInterruptedException) { }
                    catch (ThreadAbortException) { }
                }));
                tcpAccepter = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        while (true)
                        {
                            TcpClient c = listener.AcceptTcpClient();
                            clients.Add(c);
                        }
                    }
                    catch (ThreadInterruptedException) { }
                    catch (ThreadAbortException) { }
                }));
                spRead.Start();
                tcpAccepter.Start();
            }
            status.Text = "Läuft";
            button1.Tag = "stop";
            button1.Text = "Stop";
        }
        private void stop()
        {
            clients.ForEach((t) =>
            {
                try { t.Close(); }
                catch (Exception) { }
            });
            try { tcpAccepter.Abort(); }
            catch (Exception) { }
            try { spRead.Abort(); }
            catch (Exception) { }
            if (sp != null) sp.Close();

            listener.Stop();

            button1.Tag = "start";
            button1.Text = "Start";
            status.Text = "Gestoppt";
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop();
        }

        Boolean useSimulator = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            useSimulator = checkBox1.Checked;
            comboBox1.Enabled = comboBox2.Enabled = !useSimulator;
            textBox2.Enabled = button2.Enabled = useSimulator;
        }

        public SimulatorData simulatorData
        {
            get { return _simulatorData; }
            set { _simulatorData = value; }
        }
        SimulatorData _simulatorData = new SimulatorData();
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) loadFile(openFileDialog1.FileName);
        }

        private void loadFile(String file)
        {

            simulatorData.Clear();
            String[] content = System.IO.File.ReadAllLines(file, Encoding.UTF8);
            Regex validationPattern = new Regex(@"^[?-]{0,1}[0-9]+[\t ]{1}(.*)$");
            bool valid = true;
            foreach (String line in content)
            {
                if (line.Trim().Length > 0)
                {
                    if (line.Trim().StartsWith("#")) continue;
                    if (validationPattern.IsMatch(line))
                    {
                        String[] t = line.Split(new char[] { '\t', ' ' }, 2);
                        String l = t[1];
                        long d;
                        bool onlyOnce;
                        if (t[0].StartsWith("?"))
                        {
                            onlyOnce = true;
                            d = long.Parse(t[0].Substring(1));
                        }
                        else
                        {
                            onlyOnce = false;
                            d = long.Parse(t[0]);
                        }
                        simulatorData.Add(new SimData(l, d, onlyOnce));
                    }
                    else
                    {
                        valid = false;
                        simulatorData.Clear();
                        break;
                    }
                }
            }
            if (!valid)
            {
                simulatorData.Clear();
                updateListView();
                MessageBox.Show("Datei ist ungültig");
                textBox2.Text = file;
            }
            else
            {
                updateListView();
            }
        }

        public void selectListViewIndex(int idx)
        {
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new MethodInvoker(() => { selectListViewIndex(idx); }));
            }
            else
            {
                foreach (ListViewItem lvi in listView1.Items)
                {
                    lvi.BackColor = (listView1.Items.IndexOf(lvi) == idx ? Color.LightGreen : Color.White);
                }
            }
        }
        private void updateListView()
        {
            listView1.Items.Clear();
            simulatorData.ForEach((s) =>
            {
                listView1.Items.Add(s.AsListViewItem());
            });
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                SimData sd = simulatorData[listView1.SelectedIndices[0]];
                singleLineGot(sd.data);
            }
        }
    }
}
