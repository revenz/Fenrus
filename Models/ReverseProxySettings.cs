namespace Fenrus.Models;

public class ReverseProxySettings
{
    public bool DebugPrintRequestHeaders { get; set; }
    public bool UseForwardedHeaders { get; set; }
    public string[] KnownProxies { get; set; } = Array.Empty<string>();
    public KnownNetwork KnownIpv4Network { get; set; } = new();
    public KnownNetwork KnownIpv6Network { get; set; } = new();

}

public class KnownNetwork
{
    public bool Enabled { get; set; }
    public string IpAddress { get; set; }
    public int PrefixLength { get; set; }
}