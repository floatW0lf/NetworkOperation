using System;
using System.Collections.Concurrent;

namespace NetworkOperation
{
    public class AuthorizeService : IAuthorizeService
    {
        ConcurrentDictionary<long,User> _IdToUsers = new ConcurrentDictionary<long, User>();
        ConcurrentDictionary<long,User> _tokenToUser = new ConcurrentDictionary<long, User>();

        public bool CreateNewUser(long userId)
        {
            return _IdToUsers.TryAdd(userId,new User {Id = userId, LastActivity = DateTime.UtcNow});
        }

        public TokenState ValidateToken(long userId, long token)
        {
            if (_tokenToUser.TryGetValue(token, out var userByToken) && userByToken.Id != userId)
            {
                return TokenState.AlreadyExist;
            }

            if (_IdToUsers.TryGetValue(userId, out var userById) && userById.Token == token)
            {
                return TokenState.Valid;
            }

            return TokenState.Invalid;
        }

        public User GetUser(long id)
        {
            return _IdToUsers.TryGetValue(id, out var user) ? user : null;
        }

        public bool RegisterUser(long userId, string uniqData)
        {
            var user = GetUser(userId);
            if (user == null) return false;
            var token = uniqData.GetHashCode();

            user.Token = token;
            user.UniqData = uniqData;
            _tokenToUser.AddOrUpdate(token, user, (t, u) => user);

            return true;
        }

        public void UnRegisterUser(long userId)
        {
            if (_IdToUsers.TryRemove(userId, out var user))
            {
                _tokenToUser.TryRemove(user.Token, out _);
            }
        }
    }
}