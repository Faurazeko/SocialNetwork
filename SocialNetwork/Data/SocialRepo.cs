using SocialNetwork.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

            var userPostsDir = _fileStoragePath + $"\\Posts\\{local.Nickname}";
            Directory.Delete(userPostsDir, true);

            if (local != null)
                _dbContext.Users.Remove(local);
            else
            {
                _dbContext.Users.Attach(userToDelete);
                _dbContext.Users.Remove(userToDelete);
            }
        }

        public void DeleteUser(string userNickname)
        {
            if (string.IsNullOrEmpty(userNickname))
                throw new ArgumentNullException(nameof(userNickname));

            userNickname = userNickname.ToLower();

            if (!UserExists(userNickname))
                return;

            var userToDelete = new User() { Nickname = userNickname };

            var local = _dbContext.Set<User>().Local.FirstOrDefault(entry => entry.Id.Equals(userNickname));

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
            var result = _dbContext.Users.Include(u => u.Posts).ThenInclude(p => p.Ratings).FirstOrDefault(u => u.Id == userId);

            //if (result != null)
            //    _dbContext.Entry(result).Collection(u => u.Posts).Load();

            return result;
        }

        public User GetUser(string userNickname)
        {
            //var result = _dbContext.Users.FirstOrDefault(u => u.Nickname.ToLower() == userNickname.ToLower());

            //if(result != null)
            //    _dbContext.Entry(result).Collection(u => u.Posts).Load();

           var result = _dbContext.Users.Include(u => u.Posts).ThenInclude(p => p.Ratings).FirstOrDefault(u => u.Nickname.ToLower() == userNickname.ToLower());

            return result;
        }

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
    }
}
