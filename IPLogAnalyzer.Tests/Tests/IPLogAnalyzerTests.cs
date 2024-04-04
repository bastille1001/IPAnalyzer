

namespace IPLogAnalyzer.Tests.Tests
{
    public class IPLogAnalyzerTests
    {
        [Fact]
        public void GetIPAddresses_ValidLogFile_ReturnsIPAddresses()
        {
            // Arrange
            string filePath = "valid_log_file.txt";
            File.WriteAllText(filePath, "192.168.0.1 05.04.2024 15:30:45\n192.168.0.2 05.04.2024 15:31:00");

            // Act
            var ipAddresses = IPLogAnalyzer.GetIPAddresses(filePath);

            // Assert
            Assert.NotNull(ipAddresses);
            Assert.Equal(2, ipAddresses.Count);
            Assert.Equal("192.168.0.1", ipAddresses[0].IPAddress);
            Assert.Equal("192.168.0.2", ipAddresses[1].IPAddress);
        }

        [Fact]
        public void GetIPAddresses_InvalidLogFile_ReturnsEmptyList()
        {
            // Arrange
            string filePath = "invalid_log_file.txt";
            File.WriteAllText(filePath, "invalid log file content");

            // Act
            var ipAddresses = IPLogAnalyzer.GetIPAddresses(filePath);

            // Assert
            Assert.NotNull(ipAddresses);
            Assert.Empty(ipAddresses);
        }

        [Fact]
        public void FilterIPAddresses_FilterByAddressStart_ReturnsFilteredIPAddresses()
        {
            // Arrange
            var ipAddresses = new List<(string IPAddress, DateTime Time)>
            {
                ("192.168.0.1", DateTime.Now),
                ("192.168.0.2", DateTime.Now),
                ("192.168.0.3", DateTime.Now)
            };
            var arguments = ("", "", "192.168", "", 0);

            // Act
            var filteredIPAddresses = IPLogAnalyzer.FilterIPAddresses(ipAddresses, arguments);

            // Assert
            Assert.NotNull(filteredIPAddresses);
            Assert.Equal(3, filteredIPAddresses.Count);
        }

        [Fact]
        public void FilterIPAddresses_FilterByAddressMask_ReturnsFilteredIPAddresses()
        {
            // Arrange
            var ipAddresses = new List<(string IPAddress, DateTime Time)>
            {
                ("192.168.0.1", DateTime.Now),
                ("192.168.0.2", DateTime.Now),
                ("192.168.0.3", DateTime.Now)
            };
            var arguments = ("", "", "192.168.0.1", "192.168.0", 0);

            // Act
            var filteredIPAddresses = IPLogAnalyzer.FilterIPAddresses(ipAddresses, arguments);

            // Assert
            Assert.NotNull(filteredIPAddresses);
            Assert.Single(filteredIPAddresses);
            Assert.Equal("192.168.0.1", filteredIPAddresses[0].IPAddress);
        }
    }
}
