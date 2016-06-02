using EPiServer.Data;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Infrastructure.Attributes
{
    public class AllowDBWriteAttributeTests
    {
        [Fact]
        public void IsValidForRequest_WhenDatabaseModeIsReadOnly_ShouldReturnFalse()
        {
            _databaseModeMock.Setup(d => d.DatabaseMode).Returns(() => DatabaseMode.ReadOnly);
            
            var result = _subject.IsValidForRequest(null, null);

            Assert.False(result);
        }

        [Fact]
        public void IsValidForRequest_WhenDatabaseModeIsReadWrite_ShouldReturnTrue()
        {
            _databaseModeMock.Setup(d => d.DatabaseMode).Returns(() => DatabaseMode.ReadWrite);

            var result = _subject.IsValidForRequest(null, null);

            Assert.True(result);
        }

        private AllowDBWriteAttributeForTest _subject;
        private Mock<IDatabaseMode> _databaseModeMock;


        public AllowDBWriteAttributeTests()
        {
            _databaseModeMock = new Mock<IDatabaseMode>();
            _subject = new AllowDBWriteAttributeForTest();
            _subject.SetDatabaseMode(_databaseModeMock.Object);
        }

        private class AllowDBWriteAttributeForTest : AllowDBWriteAttribute
        {
            public void SetDatabaseMode(IDatabaseMode dbMode)
            {
                DBMode = new Injected<IDatabaseMode>(dbMode);
            }
        }
    }
}
