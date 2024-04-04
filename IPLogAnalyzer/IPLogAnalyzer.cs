using System.Globalization;
using System.Net;

namespace IPLogAnalyzer
{
    public class IPLogAnalyzer
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: IPLogAnalyzer --file-log <log_file_path> [--file-output <output_file_path>] [--address-start <start_ip>] [--address-mask <ip_mask>]");
                return;
            }

            var arguments = ParseArguments(args);

            var ipAddresses = GetIPAddresses(arguments.FileLog);

            var filteredIPAddresses = FilterIPAddresses(ipAddresses, arguments);

            WriteIPAddressesToFile(filteredIPAddresses, arguments.FileOutput);

            Console.WriteLine("Done!");
        }

        public static (string FileLog, string FileOutput, string AddressStart, string AddressMask, int TimeInterval) ParseArguments(string[] args)
        {
            var arguments = new Dictionary<string, string>();

            for (int i = 0; i < args.Length - 1; i += 2)
            {
                arguments.Add(args[i], args[i + 1]);
            }

            return (
                arguments.TryGetValue("--file-log", out var fileLog) ? fileLog : null,
                arguments.TryGetValue("--file-output", out var fileOutput) ? fileOutput : "output.txt",
                arguments.TryGetValue("--address-start", out var addressStart) ? addressStart : null,
                arguments.TryGetValue("--address-mask", out var addressMask) ? addressMask : null,
                arguments.TryGetValue("--time-interval", out var timeInterval) ? int.Parse(timeInterval) : 0
            );
        }

        public static List<(string IPAddress, DateTime Time)> GetIPAddresses(string filePath)
        {
            var ipAddresses = new List<(string IPAddress, DateTime Time)>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');

                    if (parts.Length >= 2)
                    {
                        string ipAddress = parts[0];
                        string dateTimeStr = string.Join(" ", parts.Skip(1));

                        if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                        {
                            ipAddresses.Add((ipAddress, time));
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse date time: {dateTimeStr}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
            }

            return ipAddresses;
        }


        public static List<(string IPAddress, DateTime Time)> FilterIPAddresses(List<(string IPAddress, DateTime Time)> ipAddresses, (string FileLog, string FileOutput, string AddressStart, string AddressMask, int TimeInterval) arguments)
        {
            var filteredIPAddresses = ipAddresses;

            if (!string.IsNullOrEmpty(arguments.AddressStart))
            {
                filteredIPAddresses = filteredIPAddresses.Where(ip => ip.IPAddress.StartsWith(arguments.AddressStart)).ToList();
            }

            if (!string.IsNullOrEmpty(arguments.AddressMask))
            {
                filteredIPAddresses = ApplyIPAddressMask(filteredIPAddresses, arguments.AddressMask);
            }

            if (arguments.TimeInterval > 0)
            {
                var currentTime = DateTime.Now;
                var startTime = currentTime.AddMinutes(-arguments.TimeInterval);
                filteredIPAddresses = filteredIPAddresses.Where(ip => ip.Time >= startTime && ip.Time <= currentTime).ToList();
            }

            return filteredIPAddresses;
        }


        public static List<(string IPAddress, DateTime Time)> ApplyIPAddressMask(List<(string IPAddress, DateTime Time)> ipAddresses, string mask)
        {
            var maskedAddresses = new List<(string IPAddress, DateTime Time)>();

            try
            {
                var ipAddress = IPAddress.Parse(mask);
                var maskBytes = ipAddress.GetAddressBytes();
                var maskBits = maskBytes.SelectMany(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToArray();

                foreach (var ip in ipAddresses)
                {
                    var addressBytes = IPAddress.Parse(ip.IPAddress).GetAddressBytes();
                    var addressBits = addressBytes.SelectMany(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToArray();

                    bool isMatch = true;
                    for (int i = 0; i < maskBits.Length; i++)
                    {
                        if (maskBits[i] == '1' && maskBits[i] != addressBits[i])
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        maskedAddresses.Add(ip);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying IP address mask: {ex.Message}");
            }

            return maskedAddresses;
        }


        public static void WriteIPAddressesToFile(List<(string IPAddress, DateTime Time)> ipAddresses, string outputPath)
        {
            try
            {
                using var writer = new StreamWriter(outputPath);
                foreach (var address in ipAddresses)
                {
                    writer.WriteLine($"{address.IPAddress} {address.Time}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to output file: {ex.Message}");
            }
        }
    }
}
