namespace Fenrus.Models;

/// <summary>
/// Configuration object that maps to the ReverseProxySettings section in appsettings.json
/// </summary>
public class ReverseProxySettings
{
    /// <summary>
    /// If the request headers should be printed to the console when being redirected to the
    /// authentication provider.
    /// </summary>
    public bool DebugPrintRequestHeaders { get; set; }
    
    /// <summary>
    /// If the forwarded headers should be used.
    /// </summary>
    public bool UseForwardedHeaders { get; set; }
    
    /// <summary>
    /// The IP-addresses of known proxies.
    /// </summary>
    public string[] KnownProxies { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// The IP-address specification of a known network in IPv4 format
    /// For example: 192.168.2.0/24, which will allow all IP-addresses from
    /// 192.168.2.1 - 192.168.2.254
    /// </summary>
    public KnownNetwork KnownIpv4Network { get; set; } = new();
    
    /// <summary>
    /// The IP-address specification of a known network in IPv6 format
    /// For example: 2001:db8::/32, which will allow all IP-addresses from
    /// 2001:db8:0:0:0:0:0:0 - 2001:db8:ffff:ffff:ffff:ffff:ffff:ffff
    /// </summary>
    public KnownNetwork KnownIpv6Network { get; set; } = new();

}

public class KnownNetwork
{
    /// <summary>
    /// If the known network should be added to the list of known networks.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// The IP-address of the known network.
    /// For example: 192.168.2.0 or 2001:db8::
    /// </summary>
    public string IpAddress { get; set; }
    
    /// <summary>
    /// The prefix length of the known network. For example: 24
    /// </summary>
    public int PrefixLength { get; set; }
}