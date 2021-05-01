namespace Fitmeplan.Identity.Security
{
    /// <summary>Minimal interface for a user with id and username</summary>
    public interface IUser
    {
        /// <summary>Unique key for the user</summary>
        long Id { get; }
        /// <summary>Unique username</summary>
        string Email { get; set; }
    }
}
