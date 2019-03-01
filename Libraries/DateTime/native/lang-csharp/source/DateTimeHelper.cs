using System;
using System.Collections.Generic;
using Interpreter.Structs;

namespace Interpreter.Libraries.DateTime
{
    internal static class DateTimeHelper
    {
        private static System.TimeZoneInfo GetTimeZone(object nullableTimeZoneAsObj)
        {
            if (nullableTimeZoneAsObj == null)
            {
                return System.TimeZoneInfo.Utc;
            }
            return (System.TimeZoneInfo)nullableTimeZoneAsObj;
        }

        private static readonly System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool UnixToStructured(int[] intOut, object nullableTimeZone, double unixTime)
        {
            System.TimeZoneInfo tz = GetTimeZone(nullableTimeZone);
            System.DateTime utcDateTime = epoch.AddSeconds(unixTime);
            System.TimeSpan utcOffset = tz.GetUtcOffset(utcDateTime);
            System.DateTime dt = utcDateTime.Add(utcOffset);

            intOut[0] = dt.Year;
            intOut[1] = dt.Month;
            intOut[2] = dt.Day;
            intOut[3] = dt.Hour;
            intOut[4] = dt.Minute;
            intOut[5] = dt.Second;
            intOut[6] = dt.Millisecond;
            intOut[7] = 0;
            intOut[8] = 1 + (int)dt.DayOfWeek;

            return true;
        }

        public static void ParseDate(int[] intOut, object nullableTimeZone, int year, int month, int day, int hour, int minute, int microseconds)
        {
            // TODO: there is a problem here. millis are dropped.
            intOut[0] = 1;
            int seconds = (int)(microseconds / 1000000);
            int millis = (microseconds / 1000) % 1000;
            System.TimeZoneInfo tz = GetTimeZone(nullableTimeZone);
            System.DateTime dt = new System.DateTime(year, month, day, hour, minute, seconds, millis, DateTimeKind.Unspecified);
            dt = TimeZoneInfo.ConvertTimeToUtc(dt, tz);
            double unixTime = dt.Subtract(epoch).TotalSeconds;
            intOut[1] = (int)unixTime;
        }

        public static bool IsDstOccurringAt(object nativeTimeZone, int unixtime)
        {
            throw new NotImplementedException();
        }

        public static object[] InitializeTimeZoneList()
        {
            throw new NotImplementedException();
        }

        public static int GetOffsetFromUtcNow(object nativeTimeZone)
        {
            throw new NotImplementedException();
        }

        public static object GetDataForLocalTimeZone(string[] strOut, int[] intOut)
        {
            System.TimeZoneInfo tzInfo = System.TimeZoneInfo.Local;
            strOut[0] = tzInfo.DisplayName.Split(')')[1].Trim();
            strOut[1] = tzInfo.Id;
            intOut[0] = -1; // Not used. Remove this.
            intOut[1] = tzInfo.SupportsDaylightSavingTime ? 1 : 0;
            return tzInfo;
        }
    }
}
