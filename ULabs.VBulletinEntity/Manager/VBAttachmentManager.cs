using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Models.Forum;

namespace ULabs.VBulletinEntity.Manager {
    public class VBAttachmentManager {
        readonly VBDbContext db;

        public VBAttachmentManager(VBDbContext db) {
            this.db = db;
        }

        public async Task<VBAttachment> GetAttachmentInfoAsync(int id) {
            // ToDo: May cause overhead when storing file content in db
            var attachment = await db.Attachments.Include(a => a.FileData)
                .SingleOrDefaultAsync(attach => attach.Id == id);
            return attachment;
        }

        public async Task<PhysicalFileResult> GetAttachmentAsync(VBAttachment attachment, string attachmentsPath) {
            string fullPath = Path.Combine(attachmentsPath, attachment.FilePath);
            var fileContentProvider = new FileExtensionContentTypeProvider();
            string mimeType;
            if (!fileContentProvider.TryGetContentType(attachment.FileName, out mimeType)) {
                mimeType = "application/octet-stream";
            }

            var file = new PhysicalFileResult(fullPath, mimeType);

            attachment.DownloadsCount++;
            await db.SaveChangesAsync();

            return file;
        }
    }
}
