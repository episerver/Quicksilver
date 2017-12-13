using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    class FakeCustomerContext : CustomerContextFacade
    {
        private readonly CurrentContactFacade _currentContact;

        public FakeCustomerContext(CurrentContactFacade currentContact) : base(null)
        {
            _currentContact = currentContact;
        }

        public override CurrentContactFacade CurrentContact
        {
            get { return _currentContact; }
        }
    }
}