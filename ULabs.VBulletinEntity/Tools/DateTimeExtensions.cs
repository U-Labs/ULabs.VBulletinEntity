using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Tools {
    public static class DateTimeExtensions {
        public static long ToUnixTimestamp(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc) {
            dateTime = dateTime.ForceUtc();
            var tsStart = new DateTime(1970, 1, 1, 0, 0, 0, kind);
            var ts = (long)(dateTime - tsStart).TotalSeconds;
            return ts;
        }

        public static int ToUnixTimestampAsInt(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc) {
            return Convert.ToInt32(dateTime.ToUnixTimestamp(kind));
        }

        public static DateTime ToDateTime(this int ts, DateTimeKind kind = DateTimeKind.Utc) {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, kind);
            return dateTime.AddSeconds(ts);
        }

        public static DateTime ForceUtc(this DateTime dateTime) {
            if (dateTime.Kind != DateTimeKind.Utc) {
                dateTime = dateTime.ToUniversalTime();
            }
            return dateTime;
        }

        public static string ToGermanHumanReadableSinceNow(this DateTime dateTime, bool longFormat = true) {
            var utc = dateTime.ForceUtc();
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - utc;

            string human = "";
            decimal val = 0;
            if (timeSpan.TotalDays >= 7) {
                val = Math.Round((decimal)timeSpan.TotalDays / 7);
                human = (longFormat ? "Wochen" : "W");
            } else if (timeSpan.TotalHours >= 24) {
                val = Math.Round((decimal)timeSpan.TotalHours / 24);
                human = (longFormat ? "Tage" : "T");
            } else if (timeSpan.TotalMinutes >= 60) {
                val = Math.Round((decimal)timeSpan.TotalMinutes / 60);
                human = (longFormat ? "Stunden" : "Std");
            } else if (timeSpan.TotalSeconds >= 60) {
                val = Math.Round((decimal)timeSpan.TotalMinutes);
                human = (longFormat ? "Minuten" : "Min");
            } else {
                val = Math.Round((decimal)timeSpan.TotalSeconds);
                human = (longFormat ? "Sekunden" : "Sek");
            }

            return $"{val} {human}";
        }
    }
}
