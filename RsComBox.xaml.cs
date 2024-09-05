using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using RsCommunication;
using static RsCommunication.RsComStr;

namespace RsCommunication
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RsComBox : UserControl
    {
        public event CommandReceivedEventHandler frameCommandReceived;

        private RsComStr rsCom;
        bool isRsConnected;

        public RsComStr RsCom
        {  get { return rsCom; } }

        public RsComBox()
        {
            InitializeComponent();
            //comboBoxRsList.ItemsSource = SerialPort.GetPortNames();
            comboBoxRsList.ItemsSource = ComPort.GetSerialPorts();
            if (comboBoxRsList.ItemsSource != null && ((List<ComPort>)comboBoxRsList.ItemsSource).Count > 0)
                comboBoxRsList.SelectedIndex = 0;
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 3000);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            List<ComPort> cp = ComPort.GetSerialPorts();
            if (!isRsConnected)
            {
                //comboBoxRsList.ItemsSource = SerialPort.GetPortNames();
                if (((List<ComPort>)comboBoxRsList.ItemsSource).Count != cp.Count)
                    comboBoxRsList.ItemsSource = cp;
                //if (CheckBoxAutoconnect.IsChecked.Value && !isRsConnected)
                //{
                //    ComPort com = cp.FindLast(c => c.vid.Equals("1915") && c.pid.Equals("520F"));
                //    if (com != null)
                //    {
                //        ComPortConnect(com);
                //        Thread.Sleep(1000);
                //        ScanRaports.Clear();
                //        rsCom.SendCapCom(CapCommand.CAP_COMMAND_DISCOVER_START);
                //    }
                //}
            }
            else
            {
                //if (!cp.Any(c => c.name == rsCom.Name))
                //    ComPortDisconnect();
            }

            //rsUpdateCnt = 0;
        }

        public void buttonRsConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isRsConnected)
            {
                try
                {
                    rsCom = new RsComStr(((ComPort)comboBoxRsList.SelectedItem).name, 115200);
                    rsCom.frameCommandReceived += RsCom_frameReceived;
                    rsCom.Start();
                    isRsConnected = true;
                    buttonRsConnect.Content = "Disconnect";
                    comboBoxRsList.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    rsCom = null;
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
            {
                isRsConnected = false;
                buttonRsConnect.Content = "Connect";
                rsCom.frameCommandReceived -= RsCom_frameReceived;
                rsCom.Stop();
                comboBoxRsList.IsEnabled = true;
            }
        }

        private void RsCom_frameReceived(RsCommand command, byte[] bytes)
        {
            if (isRsConnected && frameCommandReceived != null)
            {
                frameCommandReceived(command, bytes);
            }
        }


    }

    internal class ComPort
    {
        public string name = "";
        public string vid = "";
        public string pid = "";
        public string description = "";

        private const string vidPattern = @"VID_([0-9A-F]{4})";
        private const string pidPattern = @"PID_([0-9A-F]{4})";
        public static List<ComPort> GetSerialPorts()
        {
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM WIN32_SerialPort"))
            {
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                return ports.Select(p =>
                {
                    ComPort c = new ComPort();
                    c.name = p.GetPropertyValue("DeviceID").ToString();
                    c.vid = p.GetPropertyValue("PNPDeviceID").ToString();
                    c.description = p.GetPropertyValue("Caption").ToString();

                    Match mVID = Regex.Match(c.vid, vidPattern, RegexOptions.IgnoreCase);
                    Match mPID = Regex.Match(c.vid, pidPattern, RegexOptions.IgnoreCase);

                    if (mVID.Success)
                        c.vid = mVID.Groups[1].Value;
                    if (mPID.Success)
                        c.pid = mPID.Groups[1].Value;

                    return c;

                }).ToList();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name.ToString());
            sb.Append("[");
            sb.Append(vid.ToString());
            sb.Append(";");
            sb.Append(pid.ToString());
            sb.Append("]");
            sb.Append(description.ToString());


            return sb.ToString();// name.ToString() + "[" + vid.ToString() + ";" + pid.ToString() + "]" + description.ToString();
        }
    }

}
