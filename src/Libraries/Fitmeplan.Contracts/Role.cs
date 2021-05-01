using Fitmeplan.Common;

namespace Fitmeplan.Contracts
{
    public enum Role
    {
        [LocalizedDescription("Role_Administrator", typeof(Resource))]
        Administrator = 1,
        [LocalizedDescription("Role_SuperUser", typeof(Resource))]
        SuperUser
    }
}
