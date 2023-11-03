using Application.DTOs;
using Application.Secrets;
using Application.StaticData;
using Application.Tests.ObsoleteClasses;
using Data.DTOs;
using FluentAssertions;
using Opc.Ua;
using Serilog;

namespace Application.Tests
{
    public class OPCUAClientTests
    {
        private readonly Dictionary<NodeId, OPCUATag> TestDictionary = new();
        public OPCUAClientTests()
        {
            foreach (var OPCUATag in OPCUANotifNames.testList)
            {
                TestDictionary.Add(OPCUATag.NodeID, OPCUATag);
            }
        }

        [Fact]
        public void StartClient_SettingUpCorrect_ShouldReturnFalseError()
        {

            var opcuaClient = new OPCUAVoidConnector(new OPCUASpecDTO
                (
                    ServerAddres: OPCUAConnectionSecrets.ServerAddress,
                    ServerPort: OPCUAConnectionSecrets.ServerPort,
                    TagList: TestDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: 30
                ));

            var error = opcuaClient.StartClient();
            error.IsError.Should().BeFalse();
        }

        [Fact]
        public void StartClient_SettingUpWithoutServerAddress_ShouldReturnErrorMessage()
        {

            var opcuaClient = new OPCUAVoidConnector(new OPCUASpecDTO
                (
                    ServerAddres: string.Empty,
                    ServerPort: OPCUAConnectionSecrets.ServerPort,
                    TagList: TestDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: 30
                ));

            var error = opcuaClient.StartClient();
            error.IsError.Should().BeTrue();
            error.Message.Should().Contain("OPCUA Server Address is empty");
        }

        [Fact]
        public void StartClient_SettingUpWithoutServerPort_ShouldReturnErrorMessage()
        {

            var opcuaClient = new OPCUAVoidConnector(new OPCUASpecDTO
                (
                    ServerAddres: OPCUAConnectionSecrets.ServerAddress,
                    ServerPort: string.Empty,
                    TagList: TestDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: 30
                ));

            var error = opcuaClient.StartClient();
            error.IsError.Should().BeTrue();
            error.Message.Should().Contain("OPCUA Port Number is empty");
        }

        [Fact]
        public void StartClient_SettingUpWithWrongRenewalMinuesValues_ShouldReturnErrorMessage()
        {

            var opcuaClient = new OPCUAVoidConnector(new OPCUASpecDTO
                (
                    ServerAddres: OPCUAConnectionSecrets.ServerAddress,
                    ServerPort: OPCUAConnectionSecrets.ServerPort,
                    TagList: TestDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: -1
                ));

            var error = opcuaClient.StartClient();
            error.IsError.Should().BeTrue();
            error.Message.Should().Contain("Session Renewal Period Minutes set 0 or below");
        }

        [Fact]
        public void StartClient_SettingUpWithEmptyDictionary_ShouldReturnErrorMessage()
        {
            Dictionary<NodeId, OPCUATag> testDictionary = new();
            var opcuaClient = new OPCUAVoidConnector(new OPCUASpecDTO
                (
                    ServerAddres: OPCUAConnectionSecrets.ServerAddress,
                    ServerPort: OPCUAConnectionSecrets.ServerPort,
                    TagList: testDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: 30
                ));

            var error = opcuaClient.StartClient();
            error.IsError.Should().BeTrue();
            error.Message.Should().Contain("Tag List doesn't have any values");
        }


    }
}