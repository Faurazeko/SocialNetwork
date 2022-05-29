using SocialNetwork.Models;

namespace SocialNetwork.Data
{
    public interface ISocialRepo
    {
        bool SaveChanges();

        //User
        IEnumerable<User> GetAllUsers();
        void CreateUser(User user);
        bool UserExists(int userId);
        bool UserExists(string UserNickname);
        void DeleteUser(int userId);
        void DeleteUser(string UserNickname);
        User GetUser(int userId);
        User GetUser(string UserNickname);
        public void ChangeUserPassword(string username, string password);

        //Post
        IEnumerable<Post> GetAllPostsForUser(int userId);
        void CreatePost(Post post);
        void DeletePost(int postId);
        Post GetPost(int postId);
        bool PostExists(int postId);

        //Post emotions
        public void PostSetEmotion(int postId, int userId, bool isLikeIt);
        public void PostRemoveEmotion(int postId, int userId);
        public int[] GetPostEmotions(int postId);
        public bool? GetUserEmotion(int userId, int postId);

        //Friends
        public bool FriendRelationExists(int user1Id, int user2Id);
        public FriendsRelation GetFriendRelation(int user1Id, int user2Id);
        public void AddAsFriend(int user1Id, int user2Id, bool ignore = false);
        public void RemoveAsFriend(int user1Id, int user2Id);

        //Friends options
        public void ChangeFriendsVisibility(int userId, bool value);

        //Other
        public bool CheckUsernameAvailability(string username);
    }
}
