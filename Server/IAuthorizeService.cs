namespace NetworkOperation
{
    public enum TokenState
    {
        Invalid = 0,
        AlreadyExist = 1,
        Valid = 2
    }

    public interface IAuthorizeService
    {
        bool CreateNewUser(long userId);
        TokenState ValidateToken(long userId, long token);
        User GetUser(long id);
        bool RegisterUser(long userId, string uniqData);
        void UnRegisterUser(long userId);
    }
}