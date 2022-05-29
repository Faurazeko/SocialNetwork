using SocialNetwork.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace SocialNetwork.Data
{
    public class SocialRepo : ISocialRepo
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly string _fileStoragePath;

        public SocialRepo(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;

            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        }

        public void CreateUser(User user)
        {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if (UserExists(user.Nickname))
                return;

            _dbContext.Users.Add(user);
        }

        public void DeleteUser(int userId)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            if (!UserExists(userId))
                return;

            var userToDelete = new User() { Id = userId };

            var local = _dbContext.Set<User>().Local.FirstOrDefault(entry => entry.Id.Equals(userId));

            DeleteUserPrivate(userToDelete, local);
        }

        public void DeleteUser(string userNickname)
        {
            if (string.IsNullOrEmpty(userNickname))
                throw new ArgumentNullException(nameof(userNickname));

            if (!UserExists(userNickname))
                return;

            var userToDelete = new User() { Nickname = userNickname };

            var local = _dbContext.Set<User>().Local.FirstOrDefault(entry => entry.Nickname.Equals(userNickname, StringComparison.CurrentCultureIgnoreCase));

           DeleteUserPrivate(userToDelete, local);
        }

        /// <summary>
        /// User existense must be checked before calling this function
        /// </summary>
        /// <param name="userToDelete"></param>
        /// <param name="local"></param>
        private void DeleteUserPrivate(User userToDelete, User local)
        {
            var userPostsDir = _fileStoragePath + $"\\Posts\\{local.Nickname}";

            if (Directory.Exists(userPostsDir))
                Directory.Delete(userPostsDir, true);

            if (local != null)
                _dbContext.Users.Remove(local);
            else
            {
                _dbContext.Users.Attach(userToDelete);
                _dbContext.Users.Remove(userToDelete);
            }
        }

        public bool UserExists(int userId) => _dbContext.Users.Any(u => u.Id == userId);

        public bool UserExists(string userNickname) => _dbContext.Users.Any(u => u.Nickname.ToLower() == userNickname.ToLower());

        public User GetUser(int userId)
        {
            var result = UsersWithInclude().FirstOrDefault(u => u.Id == userId);

            return result;
        }

        public User GetUser(string userNickname)
        {
            var result = UsersWithInclude().FirstOrDefault(u => u.Nickname.ToLower() == userNickname.ToLower());

            return result;
        }

        private IIncludableQueryable<User, User> UsersWithInclude() =>
            _dbContext.Users
            .Include(u => u.Posts)
            .ThenInclude(p => p.Ratings)

            .Include(u => u.Friends1)
            .ThenInclude(u => u.User2)

            .Include(u => u.Friends2)
            .ThenInclude(u => u.User1);

        public void ChangeUserPassword(string username, string password)
        {
            var user = GetUser(username);
            user.Password = password;
        }

        public IEnumerable<User> GetAllUsers() => _dbContext.Users.ToList();

        public bool SaveChanges() => _dbContext.SaveChanges() >= 0;

        public IEnumerable<Post> GetAllPostsForUser(int userId) => _dbContext.Posts.Where(p => p.UserId == userId).ToList();

        public void CreatePost(Post post)
        {
            if(post == null)
                throw new ArgumentNullException(nameof(post));

            if (post.User == null)
                return;

            _dbContext.Posts.Add(post);
        }

        public void DeletePost(int postId)
        {
            if (!PostExists(postId))
                return;

            var postToDelete = new Post() { Id = postId };

            var local = _dbContext.Set<Post>().Local.FirstOrDefault(entry => entry.Id.Equals(postId));

            var postDir = _fileStoragePath + $"\\Posts\\{local.User.Nickname}\\{postId}";

            if(Directory.Exists(postDir))
                Directory.Delete(postDir, true);


            if (local != null)
                _dbContext.Posts.Remove(local);
            else
            {
                _dbContext.Posts.Attach(postToDelete);
                _dbContext.Posts.Remove(postToDelete);
            }
        }

        public Post GetPost(int postId) => _dbContext.Posts.Include(p => p.User).FirstOrDefault(entry => entry.Id.Equals(postId));

        public bool PostExists(int postId) => _dbContext.Posts.Any(p => p.Id == postId);

        public void PostSetEmotion(int postId, int userId, bool isLikeIt)
        {
            var existingRelation = _dbContext.PostUserRatings.Where(r => r.PostId == postId && r.UserId == userId).FirstOrDefault();

            if(existingRelation == null)
            {
                var item = new PostUserRating() { UserId = userId, PostId = postId, IsLikeIt = isLikeIt };
                _dbContext.PostUserRatings.Add(item);
            }
            else
            {
                existingRelation.IsLikeIt = isLikeIt;
            }
        }
        public void PostRemoveEmotion(int postId, int userId)
        {
            var relation = _dbContext.PostUserRatings.Where(r => r.PostId == postId && r.UserId == userId).FirstOrDefault();

            if (relation != null)
            {
                var local = _dbContext.Set<PostUserRating>().Local.FirstOrDefault(entry => entry.Id.Equals(relation.Id));

                if (local != null)
                    _dbContext.PostUserRatings.Remove(local);
                else
                {
                    _dbContext.PostUserRatings.Attach(relation);
                    _dbContext.PostUserRatings.Remove(relation);
                }
            }
        }

        public int[] GetPostEmotions(int postId)
        {
            int likesCount = 0;
            int dislikesCount = 0;

            foreach (var item in _dbContext.PostUserRatings.Where(p => p.PostId == postId))
            {
                if(item.IsLikeIt)
                    likesCount++;
                else
                    dislikesCount++;
            }

            return new int[] { likesCount, dislikesCount };
        }

        public bool? GetUserEmotion(int userId, int postId)
        {
            var item = _dbContext.PostUserRatings.Where(i => i.UserId == userId && i.PostId == postId).FirstOrDefault();

            if(item == null)
                return null;

            return item.IsLikeIt;
        }

        public bool FriendRelationExists(int user1Id, int user2Id) => 
            _dbContext.FriendsRelations.Any(i =>
            (i.User1Id == user1Id && i.User2Id == user2Id) ||
            (i.User1Id == user2Id && i.User2Id == user1Id));

        public FriendsRelation GetFriendRelation(int user1Id, int user2Id) =>
            _dbContext.FriendsRelations
                .Include(r => r.User1)
                .Include(r => r.User2)
                .Where(i =>
                    (i.User1Id == user1Id && i.User2Id == user2Id) ||
                    (i.User1Id == user2Id && i.User2Id == user1Id)
                )
                .FirstOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user1Id">Initializator of action</param>
        /// <param name="user2Id"></param>
        public void AddAsFriend(int user1Id, int user2Id, bool ignore = false)
        {
            var existingRelation = GetFriendRelation(user1Id, user2Id);

            if (existingRelation != null)
            {
                existingRelation.IsApproved = true;
                return;
            }

            var relation = new FriendsRelation() { User1Id = user1Id, User2Id = user2Id, IsApproved = false, IsIgnored = ignore };
            _dbContext.FriendsRelations.Add(relation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user1Id">Initializator of action</param>
        /// <param name="user2Id"></param>
        public void RemoveAsFriend(int user1Id, int user2Id)
        {
            var relation = GetFriendRelation(user1Id, user2Id);

            if (relation == null)
                return;

            _dbContext.Entry(relation).State = EntityState.Deleted;

            /*var local = _dbContext.Set<FriendsRelation>().Local.FirstOrDefault(i =>
            (i.User1Id == user1Id && i.User2Id == user2Id) ||
            (i.User1Id == user2Id && i.User2Id == user1Id));

            if (local != null)
                _dbContext.FriendsRelations.Remove(local);
            else
            {
                _dbContext.FriendsRelations.Attach(relation);
                _dbContext.FriendsRelations.Remove(relation);
            }*/
        }

        public void ChangeFriendsVisibility(int userId, bool value)
        {
            var user = GetUser(userId);
            user.IsFriendsHidden = value;
        }

        public bool CheckUsernameAvailability(string username)
        {
            username = username.ToLower();

            string[] restrictedNames = { "id0", "settings", "admin", "friend", "friends", "groups", "group", "message", "messages", "feed", "" };

            if (restrictedNames.Contains(username))
                return false;

            if (GetUser(username) != null)
                return false;

            return true;
        }

    }
}
