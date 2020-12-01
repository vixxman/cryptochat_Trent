using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using CyptoProtocols_1;

namespace Trent3_6
{
    public partial class Form1 : Form
    {
        private Socket socket;
        private EndPoint localPoint, remotePointA, remotePointB;
        private Socket handlerA, handlerB;
        private bool initNGA = true, initXA = true, initKA = true;
        private bool initNGB = true, initXB = true, initKB = true;
        private bool checkA = false, checkB = false;

        int sesKey, Ra;

        int nA, gA, xA, XA, XFA, KiA;
        int nB, gB, xB, XB, XFB, KiB;

        string[] stage1;

        int AtKey;
        int BtKey;


        string nameA = "";
        string nameB = "";


        public Form1()
        {
            InitializeComponent();
            
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            localPoint = new IPEndPoint(IPAddress.Parse(GetLocalIp()), 82);
            remotePointA = new IPEndPoint(IPAddress.Parse(GetLocalIp()), 80);
            remotePointB = new IPEndPoint(IPAddress.Parse(GetLocalIp()), 81);
            socket.Bind(localPoint);
            getSessionKey();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            ASCIIEncoding aSCII = new ASCIIEncoding();
            IPEndPoint snd = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = snd;
            byte[] buf;
            button1.Enabled = false;
            while (true)
            {
                buf = new byte[128];
                int size = socket.ReceiveFrom(buf, ref Remote);
                if (size > 0)
                {
                    string[] str = aSCII.GetString(buf).Split(':');
                    nameA = str[0];
                    nameB = str[1];
                    Int32.TryParse(str[2], out Ra);
                    AtKey = (nameA + "trent").GetHashCode() & 0x7FFFFFFF;
                    BtKey = (nameB + "trent").GetHashCode() & 0x7FFFFFFF;
                    string s = sesKey.ToString() + ':' + nameA;
                    byte[] crS = crypt(s, BtKey);
                    byte[] crS2 = crypt(Ra.ToString()+':' + nameB+':' + sesKey.ToString()+':', AtKey);
                    byte[] crS3 = crypt(crS, AtKey);
                    byte[] newArray = new byte[crS2.Length + crS3.Length];
                    crS2.CopyTo(newArray, 0);
                    crS3.CopyTo(newArray, crS2.Length);
                    socket.SendTo(newArray, Remote);
                    button1.Enabled = true;
                    return;
                }
                
            }
        }
        


        private string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        
        private void getSessionKey()
        {
            Random r = new Random();
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            sesKey =unixTime+ r.Next(99999);
        }


        private byte[] crypt(string s, int k)
        {
            ASCIIEncoding asc = new ASCIIEncoding();
            byte[] encod = asc.GetBytes(s);
            byte[] bs = BitConverter.GetBytes(k);
            for (int i = 0; i < encod.Length; i++)
            {
                encod[i] = (byte)(encod[i] ^ bs[0]);
            }
            return encod;
        }

        private byte[] crypt(byte[] s, int k)
        {
            byte[] bs = BitConverter.GetBytes(k);
            for (int i = 0;  i < s.Length; i++)
            {
                s[i] = (byte)(s[i] ^ bs[0]);
            }
            return s;
        }

    }
}
