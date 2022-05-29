using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Nickname { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool IsAdmin { get; set; } = false;
        [Required]
        public bool IsBlocked { get; set; } = false;
        [Required]
        public string AboutText { get; set; } = "";
        [Required]
        public int Trollars { get; set; } = 0;
        [DataType("datetime2")]
        public DateTime LastOnlineTime { get; set; }
        public bool IsOnline { get; set; }
        [DataType("datetime2")]
        public DateTime CreatedDateTime { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        [DataType("datetime2")]
        public DateTime ForcedLogOutTime { get; set; } = DateTime.MinValue;
        public ICollection<FriendsRelation> Friends1 { get; set; } = new List<FriendsRelation>();
        public ICollection<FriendsRelation> Friends2 { get; set; } = new List<FriendsRelation>();
        public ICollection<FriendsRelation> Friends { 
            get {
                var result = new List<FriendsRelation>();
                result.AddRange(Friends1);
                result.AddRange(Friends2);
                
                return result;
            }
        }
        public string AvatarPath { get; set; } = "/FileStorage/Default/avatar.png";

        //Settings
        public bool IsFriendsHidden { get; set; } = false;
        public bool IsHiddenInFriends { get; set; } = false;
        public bool IsOnlineHidden { get; set; } = false;
    }
}
