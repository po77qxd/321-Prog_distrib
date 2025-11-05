using System;
using System.Net;
using System.Net.Sockets;

namespace ntp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ntpServer = "0.ch.pool.ntp.org";

            byte[] timeMessage = new byte[48];
            timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);

            UdpClient client = new UdpClient();
            client.Connect(ntpReference);

            client.Send(timeMessage, timeMessage.Length);
            timeMessage = client.Receive(ref ntpReference);
            DateTime ntpTime = NtpPacket.ToDateTime(timeMessage);

            Console.WriteLine($"Heure actuelle : {ntpTime.ToLongDateString()}");
            Console.WriteLine($"Heure actuelle : {ntpTime}");
            Console.WriteLine($"Heure actuelle : {ntpTime.ToShortDateString()}");

            Console.WriteLine($"Heure actuelle : {ntpTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            client.Close();
        }
    }

    class NtpPacket()
    {
        public static DateTime ToDateTime(byte[] ntpData)
        {
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
    }
}
