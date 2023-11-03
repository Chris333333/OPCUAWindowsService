using Data.DTOs;
using Data.Extensions;
using Data.Scaffolded;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace Data.Tests
{
    public class MySQLDataSenderExtensionTests
    {

        [Fact]
        public void UpdateOneLayoutInDatabase_SettingUpCorrectUpdateOnDatabase_ShouldNotReturnAnyErrorsAndValuesAreInDatabase()
        {
            //Arrange

            var context = new DataBaseContext(new DbContextOptionsBuilder<DataBaseContext>().UseInMemoryDatabase("TestDatabase").Options);

            context.OpcuaData.Add(new Domain.Scaffolded.OpcuaDatum
            {
                DisplayName = "test",
                NodeId = "ns=1;s=test"
            });
            context.SaveChanges();

            var tag = new OPCUATag(
                displayName: "test",
                nodeID: "ns=1;s=test",
                lastUpdatedTime: DateTime.UtcNow,
                lastSourceTimeStamp: DateTime.UtcNow,
                statusCode: "StatusCode test",
                lastGoodValue: "LastGoodValue test",
                currentValue: "CurrentValue test"
                );

            //Act
            var error = MySQLDataSenderExtension.UpdateOneLayoutInDatabase(tag, context);
            var result = context.OpcuaData.SingleOrDefault(x => x.NodeId == tag.NodeID);


            //Assert
            error.Should().NotBeNull();
            error.IsError.Should().BeFalse();
            result.Should().NotBeNull();
            result.CurrentValue.Should().Contain(tag.CurrentValue);

        }

        [Fact]
        public void UpdateOneLayoutInDatabase_DBContextIsNull_ShouldReturnErrorMessage()
        {
            //Arrange
            DataBaseContext context = null;

            var tag = new OPCUATag(
                displayName: "test",
                nodeID: "ns=1;s=test",
                lastUpdatedTime: DateTime.UtcNow,
                lastSourceTimeStamp: DateTime.UtcNow,
                statusCode: "StatusCode test",
                lastGoodValue: "LastGoodValue test",
                currentValue: "CurrentValue test"
                );

            //Act
            var error = MySQLDataSenderExtension.UpdateOneLayoutInDatabase(tag, context);


            //Assert
            error.Should().NotBeNull();
            error.IsError.Should().BeTrue();
            error.Message.Should().Contain("DbContext is null when function to update one layout is called");

        }

        [Fact]
        public void UpdateOneLayoutInDatabase_NullOPCUATag_ShouldReturnErrorMessage()
        {
            //Arrange
            var context = new DataBaseContext(new DbContextOptionsBuilder<DataBaseContext>().UseInMemoryDatabase("TestDatabase2").Options);

            context.OpcuaData.Add(new Domain.Scaffolded.OpcuaDatum
            {
                DisplayName = "test",
                NodeId = "ns=1;s=test"
            });
            context.SaveChanges();

            var tag = new OPCUATag(
                displayName: "test",
                nodeID: "ns=1;s=test",
                lastUpdatedTime: DateTime.UtcNow,
                lastSourceTimeStamp: DateTime.UtcNow,
                statusCode: "StatusCode test",
                lastGoodValue: "LastGoodValue test",
                currentValue: "CurrentValue test"
                );

            //Act
            var error = MySQLDataSenderExtension.UpdateOneLayoutInDatabase(tag, context);
            var result = context.OpcuaData.SingleOrDefault(x => x.NodeId == tag.NodeID);


            //Assert
            error.Should().NotBeNull();
            error.IsError.Should().BeFalse();
            result.Should().NotBeNull();
            result.CurrentValue.Should().Contain(tag.CurrentValue);

        }

    }
}