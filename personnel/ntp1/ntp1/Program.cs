using System;
using System.Net;
using System.Net.Sockets;

namespace ntp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] ntpServers = {
                "0.pool.ntp.org",
                "1.pool.ntp.org",
                "time.google.com",
                "time.cloudflare.com"
            };

            byte[] timeMessage = new byte[48];
            timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            List<IPEndPoint> ntpReferences = new List<IPEndPoint> { };

            foreach (string server in ntpServers)
            {
                ntpReferences.Add(new IPEndPoint(Dns.GetHostAddresses(server)[0], 123));
            }


            ntpReferences.ForEach(ntpReference =>
            {

                try
                {
                    using (UdpClient client = new UdpClient())
                    {

                        client.Client.ReceiveTimeout = 1000;//1sec
                        client.Connect(ntpReference);

                        client.Send(timeMessage, timeMessage.Length);
                        timeMessage = client.Receive(ref ntpReference);
                        client.Close();
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });

            DateTime ntpTime = NtpPacket.ToDateTime(timeMessage);
            
            Console.WriteLine($"Heure actuelle : {ntpTime.ToLongDateString()}");
            Console.WriteLine($"Heure actuelle : {ntpTime}");
            Console.WriteLine($"Heure actuelle : {ntpTime.ToShortDateString()}");

            Console.WriteLine($"Heure actuelle : {ntpTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");


            TimeSpan timeDiff = DateTime.UtcNow - ntpTime;
            Console.WriteLine($"Différence de temps entre local et ntp: { timeDiff.TotalSeconds }");

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTime, TimeZoneInfo.Local);
            Console.WriteLine($"Heure locale: {localTime}");

            TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime swissTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTime, swissTimeZone);
            Console.WriteLine($"Heure suisse : {swissTime}");

            TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
            DateTime backToUtc = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, utcTimeZone);
            Console.WriteLine($"Retour vers UTC : {backToUtc}");

            DisplayWorldClocks(ntpTime);
        }

        public static void DisplayWorldClocks(DateTime utcTime)
        {
            var timeZones = new[]
            {
                ("UTC", TimeZoneInfo.Utc),
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Tokyo", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                ("Sydney", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
            };

            foreach (var (name, tz) in timeZones)
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
                Console.WriteLine($"{name}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
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
