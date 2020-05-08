using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Shared.Permission {
    /// <summary>
    /// Forum permission from includes/xml/bitfield_vbulletin.xml
    /// </summary>
    // ToDo: Implement other bitfields from the xml file
    [Flags]
    public enum VBAdminFlags : int {
        None = 0,
        IsModerator = 1
        // ToDo: Implement other permissions
    }
}
