package org.crayonlang.libraries.datetime;

import org.crayonlang.interpreter.structs.*;

public class DateTimeHelper {
	
	private static java.util.TimeZone utcTimeZone = null;
	private static java.util.TimeZone getTimeZone(Object nullableTimeZone) {
		if (nullableTimeZone == null) {
			if (utcTimeZone == null) {
				utcTimeZone = java.util.TimeZone.getTimeZone("UTC");
			}
			return utcTimeZone;
		}
		return (java.util.TimeZone) nullableTimeZone;
	}
	
	public static Object getDataForLocalTimeZone(String[] strOut, int[] intOut) {
		java.util.TimeZone tz = java.util.Calendar.getInstance().getTimeZone();
		String name = tz.getDisplayName();
		strOut[0] = name;
		strOut[1] = tz.toZoneId().getId();
		intOut[0] = -1;
		intOut[1] = tz.observesDaylightTime() ? 1 : 0;
		return tz;
	}
	
	public static int getOffsetFromUtcNow(Object nativeTz) {
		throw new RuntimeException("Not implemented.");
	}
	
	public static boolean unixToStructured(int[] intOut, Object nullableTimeZone, double unixTime) {
		java.util.TimeZone tz = getTimeZone(nullableTimeZone);
		int unixInt = (int) unixTime;
		int micros = (int)(1000000 * (unixTime - unixInt));
		int millis = micros / 1000;
		java.util.Date t = new java.util.Date(1000L * (long) unixInt);
		java.time.ZonedDateTime zdt = t.toInstant().atZone(tz.toZoneId());
		java.time.LocalDate ld = zdt.toLocalDate();
		java.time.LocalTime lt = zdt.toLocalTime();
		
		intOut[0] = ld.getYear();
		intOut[1] = ld.getMonthValue();
		intOut[2] = ld.getDayOfMonth();
		intOut[3] = lt.getHour();
		intOut[4] = lt.getMinute();
		intOut[5] = lt.getSecond();
		intOut[6] = millis;
		intOut[7] = micros;
		intOut[8] = 1 + ld.getDayOfWeek().getValue();
		
		return true;
	}
	
	private static String pad(int value, int size) {
		String output = value + "";
		while (output.length() < size) {
			output = "0" + output;
		}
		return output;
	}
	
	private static java.time.format.DateTimeFormatter parseFormat = java.time.format.DateTimeFormatter.ofPattern("y M d H m s VV");
	public static void parseDate(int[] intOut, Object nullableTimeZone, int year, int month, int day, int hour, int minute, int microseconds) {
		intOut[0] = 1;
		int seconds = (int)(microseconds / 1000000);
		int micros = microseconds - seconds * 1000000;
		intOut[2] = micros;
		boolean isUtc = nullableTimeZone == null;
		java.util.TimeZone tz = getTimeZone(nullableTimeZone);
		String tzId = tz.toZoneId().getId();
		java.time.ZonedDateTime zdt;
		if (isUtc) {
			String tsIso = year + " " + pad(month, 2) + " " + pad(day, 2) + " " + pad(hour, 2) + " " + pad(minute, 2) + ":" + pad(day, 2) + "Z";
			zdt = java.time.ZonedDateTime.parse(tsIso);
		} else {
			String tsParse = year + " " + month + " " + day + " " + hour + " " + minute + " " + seconds + " " + tz.toZoneId().getId();
			zdt = java.time.ZonedDateTime.parse(tsParse, parseFormat);
		}
		intOut[1] = (int) zdt.toEpochSecond();
	}
	
	public static Object[] initializeTimeZoneList() {
		throw new RuntimeException("Not implemented.");
	}
	
	public static boolean isDstOccurringAt(Object nullableTimeZone, int unixTime) {
		throw new RuntimeException("Not implemented.");
	}
}
