namespace Settings;

public static class Settings
{
	public static int maxErrorWaitTime = 60;
	public static int initialErrorWaitTime = 10;
	public static int errorWaitStep = 10;
	public static int[] retryCodes = {500, 502, 503, 504, 408};

	public static int maxConcurrency = 16;
}
