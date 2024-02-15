using System.ComponentModel.DataAnnotations;

namespace InterviewSathi.Web.Models.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
