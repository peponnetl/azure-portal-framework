using NFluent;
using System;
using Moq;
using Moq.Protected;
using Xunit;

namespace Azure.LogAnalyticsWorkspace.UnitTests
{
    public class AzureLoggerUnitTests
    {
        [Fact]
        public void AzureLoggerUnitTests_WorkspaceId_Exception()
        {
            var workspaceId = string.Empty;
            var sharedKey = "sharedKey";

            var expectedException = new ArgumentException($"Can not be empty", nameof(workspaceId));

            Check.ThatCode(() => new AzureLogger(workspaceId, sharedKey))
                .ThrowsType(typeof(ArgumentException))
                .WithMessage(expectedException.Message);
        }

        [Fact]
        public void AzureLoggerUnitTests_SharedKey_Exception()
        {
            var workspaceId = "workspaceId";
            var sharedKey = string.Empty;

            var expectedException = new ArgumentException($"Can not be empty", nameof(sharedKey));

            Check.ThatCode(() => new AzureLogger(workspaceId, sharedKey))
                .ThrowsType(typeof(ArgumentException))
                .WithMessage(expectedException.Message);
        }

        [Fact]
        public void AzureLoggerUnitTests_numThreads_Exception()
        {
            var workspaceId = "workspaceId";
            var sharedKey = "sharedKey";
            var numThreads = 0;

            var expectedException = new ArgumentException($"Must be greater than 0", nameof(numThreads));

            Check.ThatCode(() => new AzureLogger(workspaceId, sharedKey, numThreads))
                .ThrowsType(typeof(ArgumentException))
                .WithMessage(expectedException.Message);
        }

        [Fact]
        public void AzureLoggerUnitTests_Ok()
        {
            var workspaceId = "workspaceId";
            var sharedKey = "sharedKey";
            var numThreads = 4;

            var mockAzureLogger = new Mock<AzureLogger>(workspaceId, sharedKey, numThreads);

            mockAzureLogger.Protected().Setup("Enqueue", ItExpr.IsAny<LogEvent>()).Verifiable();

            mockAzureLogger.Object.LogInfo("logType", "username", "message", "jsonPayload");

            mockAzureLogger.Verify();
        }
    }
}