using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("created_time")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("recipient_user_id")]
        public int? RecipientUserId { get; set; }

        [Column("recipient_role")]
        [StringLength(50)]
        public string? RecipientRole { get; set; }
    }
}
