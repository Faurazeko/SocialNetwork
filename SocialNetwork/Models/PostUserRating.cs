namespace SocialNetwork.Models
{
    public class PostUserRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public bool IsLikeIt { get; set; }
    }
}
