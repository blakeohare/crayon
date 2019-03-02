import calendar
import datetime

def lib_datetime_getDataForLocalTimeZone(strOut, intOut):
	if os.path.exists('/etc/timezone'):
		tzName = file('/etc/timezone').read().strip()
	elif os.name == 'nt' and os.path.exists(r"C:\Windows\System32\tzutil.exe"):
		c = os.popen('tzutil /g')
		tzName = c.read().strip()
		c.close()
	else:
		tzName = "Local time zone"
	
	strOut[0] = tzName
	strOut[1] = tzName
	observesDst = time.daylight != 0
	intOut[0] = -1
	intOut[1] = 1 if time.daylight != 0 else 0
	
	return {
		"isLocal": True,
		"name": tzName,
		"hasDst": observesDst
	}

def lib_datetime_unixToStructuredImpl(intOut, nullableTimeZone, unixTime):
	
	unixTimeInt = int(unixTime)
	micros = int((unixTime - unixTimeInt) * 1000000)
	
	intOut[6] = int(micros / 1000)
	intOut[7] = micros
	if nullableTimeZone == None:
		ts = datetime.datetime.utcfromtimestamp(unixTimeInt)
	else:
		ts = datetime.datetime.fromtimestamp(unixTimeInt)
	
	intOut[0] = ts.year
	intOut[1] = ts.month
	intOut[2] = ts.day
	intOut[3] = ts.hour
	intOut[4] = ts.minute
	intOut[5] = ts.second
	intOut[8] = ts.isoweekday() + 1
	
	return True

def lib_datetime_parseDateImpl(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds):
	intOut[0] = 1
	
	second = int(microseconds / 1000000)
	microseconds -= second * 1000000
	intOut[2] = microseconds
	if nullableTimeZone != None:
		dt = datetime.datetime(year, month, day, hour, minute, second)
		intOut[1] = int(time.mktime(dt.timetuple()))
	else:
		tt = (year, month, day, hour, minute, second, -1, -1, -1)
		intOut[1] = int(calendar.timegm(tt))

def lib_datetime_getUtcOffsetAtImpl(tz, unixtime):
	dateTimeUtc = datetime.datetime.utcfromtimestamp(unixtime)
	dateTimeLoc = datetime.datetime.fromtimestamp(unixtime)
	locMins = (dateTimeLoc.hour * 60 + dateTimeLoc.minute)
	utcMins = (dateTimeUtc.hour * 60 + dateTimeUtc.minute)
	if dateTimeUtc.day + 1 == dateTimeLoc.day:
		utcMins -= 24 * 60
	elif dateTimeUtc.day - 1 == dateTimeLoc.day:
		utcMins += 24 * 60
	elif dateTimeUtc.day < dateTimeLoc.day: # if it's less but not 1-off, then it's actually the next month
		utcMins += 24 * 60
	elif dateTimeUtc.day > dateTimeLoc.day:
		utcMins -= 24 * 60
	diffMins = locMins - utcMins
	return int(diffMins * 60)
