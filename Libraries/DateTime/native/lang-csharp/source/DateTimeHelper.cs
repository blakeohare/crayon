using System;
using System.Collections.Generic;
using Interpreter.Structs;

namespace Interpreter.Libraries.DateTime
{
    internal static class DateTimeHelper
    {
        private static TimeZoneInfo GetTimeZone(object nullableTimeZoneAsObj)
        {
            if (nullableTimeZoneAsObj == null)
            {
                return TimeZoneInfo.Utc;
            }
            return (TimeZoneInfo)nullableTimeZoneAsObj;
        }

        private static readonly System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool UnixToStructured(int[] intOut, object nullableTimeZone, double unixTime)
        {
            int unixTimeInt = (int)unixTime;
            int micros = (int)((unixTime - unixTimeInt) * 1000000);
            TimeZoneInfo tz = GetTimeZone(nullableTimeZone);
            System.DateTime utcDateTime = epoch.AddSeconds(unixTimeInt);
            TimeSpan utcOffset = tz.GetUtcOffset(utcDateTime);
            System.DateTime dt = utcDateTime.Add(utcOffset);

            intOut[0] = dt.Year;
            intOut[1] = dt.Month;
            intOut[2] = dt.Day;
            intOut[3] = dt.Hour;
            intOut[4] = dt.Minute;
            intOut[5] = dt.Second;
            intOut[6] = micros / 1000;
            intOut[7] = micros;
            intOut[8] = 1 + (int)dt.DayOfWeek;

            return true;
        }

        public static void ParseDate(int[] intOut, object nullableTimeZone, int year, int month, int day, int hour, int minute, int microseconds)
        {
            intOut[0] = 1;
            int seconds = (int)(microseconds / 1000000);
            int micros = microseconds - seconds * 1000000;
            TimeZoneInfo tz = GetTimeZone(nullableTimeZone);
            System.DateTime dt = new System.DateTime(year, month, day, hour, minute, seconds, 0, DateTimeKind.Unspecified);
            dt = TimeZoneInfo.ConvertTimeToUtc(dt, tz);
            double unixTime = dt.Subtract(epoch).TotalSeconds;
            intOut[1] = (int)unixTime;
            intOut[2] = micros;
        }

        public static bool IsDstOccurringAt(object nativeTimeZone, int unixtime)
        {
            throw new NotImplementedException();
        }

        public static object[] InitializeTimeZoneList()
        {
            throw new NotImplementedException();
        }

        public static int GetUtcOffsetAt(object nativeTimeZone, int unixTime)
        {
            TimeZoneInfo tz = GetTimeZone(nativeTimeZone);
            System.DateTime dt = new System.DateTime(1L * unixTime * TimeSpan.TicksPerSecond);
            TimeSpan offset = tz.GetUtcOffset(dt);
            int offsetSeconds = (int)offset.TotalSeconds;
            return offsetSeconds;
        }

        public static object GetDataForLocalTimeZone(string[] strOut, int[] intOut)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.Local;
            strOut[0] = tzInfo.DisplayName.Split(')')[1].Trim();
            strOut[1] = tzInfo.Id;
            intOut[0] = -1; // Not used. Remove this.
            intOut[1] = tzInfo.SupportsDaylightSavingTime ? 1 : 0;
            return tzInfo;
        }
    }
}
