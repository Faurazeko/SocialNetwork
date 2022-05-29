using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
    public class FriendsRelation
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public int User1Id { get; set; }
        /// <summary>
        /// User that need to approve request
        /// </summary>
        public User User1 { get; set; }
        public int User2Id { get; set; }
        /// <summary>
        /// User that send the request
        /// </summary>
        public User User2 { get; set; }
        /// <summary>
        /// Is friend request approved by User1?
        /// </summary>
        public bool IsApproved { get; set; }
        public bool IsIgnored { get; set; }
    }
}
