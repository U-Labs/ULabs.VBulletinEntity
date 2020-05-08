using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ULabs.VBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("poll")]
    public class VBPoll {
        [Column("pollid")]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Question { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [Column("options")]
        public string OptionsRaw { get; set; }

        [Column("votes")]
        public string VotesRaw { get; set; }

        [Column("active")]
        public bool IsActive { get; set; }

        [Column("numberoptions")]
        public int OptionsCount { get; set; }

        [Column("timeout")]
        public int TimeoutDays { get; set; }

        [Column("multiple")]
        public bool MultipleOptionsPossible { get; set; }

        [Column("voters")]
        public int TotalVotersCount { get; set; }

        [Column("public")]
        public int IsPublic { get; set; }

        [Column("lastvote")]
        public int LastVoteTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastVoteTime {
            get { return LastVoteTimeRaw.ToDateTime(); }
            set { LastVoteTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [NotMapped]
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [NotMapped]
        public DateTime Timeout {
            get { return CreatedTime.AddMonths(TimeoutDays).ForceUtc(); }
            set { TimeoutDays = (int)value.Subtract(CreatedTime).TotalDays; }
        }
    }
}
