using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config {
    [Table("setting")]
    public class VBSettings {
        [Column("varname"), Key]
        public string Name { get; set; }

        [MaxLength(50)]
        public string GroupTitle { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }

        public string OptionCode { get; set; }

        public int DisplayOrder { get; set; }

        public int Advanced { get; set; }

        public int Volatile { get; set; }

        [Column("datatype")]
        public string DataTypeRaw { get; set; }

        [MaxLength(25)]
        public string Product { get; set; }

        public string ValidationCode { get; set; }
        public int Blacklist { get; set; }

        [NotMapped]
        public string StringValue {
            get { return Value.ToString(); }
        }

        [NotMapped]
        public int IntValue {
            get { return int.Parse(StringValue); }
        }

        [NotMapped]
        public VBSettingsType DataType {
            get {
                var names = Enum.GetNames(typeof(VBSettingsType))
                    .ToList();
                if (Enum.TryParse(DataTypeRaw, out VBSettingsType settingsType))
                    return settingsType;
                throw new Exception($"Invalid settings data type: {DataTypeRaw}");
            }
            set { DataTypeRaw = value.ToString().ToLower(); }
        }
    }
}
