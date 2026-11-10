namespace SR.Mobile;

public static class Configuracion
{
    public const bool UsarApiOnline = true;

    public const string ApiOnline = "https://srrgestion.runasp.net/";

    public static string ApiUrl =>
        UsarApiOnline ? ApiOnline
        : DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5080/"
        : "https://localhost:7080/";
}
