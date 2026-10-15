namespace MauiApp1_Assignment1_.Services;

public static class ApiSettings
{
    //must match the port of the http server, which is set in the launchSettings.json file of the API project
    private const int Port = 5108;

    // Android emulator: 10.0.2.2 means "the PC running the emulator".
    // Windows app / iOS simulator: localhost is the PC itself.
    // Real phone: replace with your PC's network address, e.g. "http://192.168.1.20:5108/"

    private const string PcAddress = "192.168.0.105";

    public static string BaseUrl
    {
        get
        {
            bool isEmulator = DeviceInfo.DeviceType == DeviceType.Virtual;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return isEmulator
                    ? $"http://10.0.2.2:{Port}/"       // Android emulator
                    : $"http://{PcAddress}:{Port}/";   // Real Android device
            }

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return isEmulator
                    ? $"http://localhost:{Port}/"      // iOS simulator
                    : $"http://{PcAddress}:{Port}/";   // Real iOS device
            }

            return $"http://localhost:{Port}/";        // Windows app or other platforms
        }
    }
}
