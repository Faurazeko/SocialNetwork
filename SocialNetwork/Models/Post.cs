using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Models
{
    public class Post
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [DataType("datetime2")]
        public DateTime CreatedDate { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        public string FilesDirsSerialized { get; private set; } = "";
        [NotMapped]
        public string[] FilesDirs { 
            get => FilesDirsSerialized.Split(';');

            set
            {
                if(value == null || value.Length == 0)
                    FilesDirsSerialized = "";
                else
                    FilesDirsSerialized = String.Join(";", value);

            }
        }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedTime { get; set; }
        public ICollection<PostUserRating> Ratings { get; set; } = new List<PostUserRating>();
        public int GetLikesCount() => Ratings.Where(r => r.IsLikeIt == true).Count();
        public int GetDislikesCount() => Ratings.Where(r => r.IsLikeIt == false).Count();
        public bool? GetEmotionForUser(int userId)
        {
            var rating = Ratings.Where(r => r.UserId == userId).FirstOrDefault();

            if (rating == null)
                return null;

            return rating.IsLikeIt;
        }
    }
}
