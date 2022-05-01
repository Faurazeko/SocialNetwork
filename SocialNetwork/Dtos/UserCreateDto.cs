using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Dtos
{
    public class UserCreateDto
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
}
