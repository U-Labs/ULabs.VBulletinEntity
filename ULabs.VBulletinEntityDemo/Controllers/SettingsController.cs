using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class SettingsController : Controller {
        readonly VBSettingsManager settingsManager;
        public SettingsController(VBSettingsManager settingsManager) {
            this.settingsManager = settingsManager;
        }
        public IActionResult Index() {
            var settings = settingsManager.GetCommonSettings();
            return View(settings);
        }
    }
}