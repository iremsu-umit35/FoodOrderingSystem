using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("ReviewReply")]
    public class ReviewReply
    {
        [Key]
        public int ReplyId { get; set; }

        public int ReviewId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ReplyText { get; set; }

        public DateTime RepliedAt { get; set; }

        // Navigation Property (opsiyonel ama güzel olur)
        public Review Review { get; set; }
    }
}
