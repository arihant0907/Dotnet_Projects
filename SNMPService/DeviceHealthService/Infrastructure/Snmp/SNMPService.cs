using DeviceHealthService.Domain.Entities;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib;
using Mono.Options;
using Google.Protobuf.Compiler;
using Mysqlx.Expr;
using Mysqlx.Session;
using static Mysqlx.Notice.Warning.Types;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace DeviceHealthService.Infrastructure.Snmp
{
    public class SNMPService
    {
        private readonly ILogger<SNMPService> logger;


        public SNMPService(ILogger<SNMPService> _logger)
        {
            logger = _logger;
        }

        public async Task<List<Variable>> GetAsync(SnmpDevice device, int timeout = 5000)
        {
            IPAddress? ip=null;
            int port = 161; // Default SNMP port
            string hostNameOrAddress = device.IpAddress;
            string community = "public";
            VersionCode version = VersionCode.V2;

            int retry = 0;
            Levels level = Levels.Reportable;
            string user = string.Empty;
            string contextName = string.Empty;
            string authentication = string.Empty;
            string authPhrase = string.Empty;
            string privacy = string.Empty;
            string privPhrase = string.Empty;
            bool dump = false;

            List<Variable> result = new List<Variable>();

            // Handle host:port format
            if (hostNameOrAddress.Contains(':'))
            {
                string[] parts = hostNameOrAddress.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int parsedPort))
                {
                    hostNameOrAddress = parts[0];
                    port = parsedPort;
                }
            }

            bool parsed = IPAddress.TryParse(hostNameOrAddress, out ip);
            if (!parsed)
            {
                var addresses = Dns.GetHostAddressesAsync(hostNameOrAddress);
                addresses.Wait();
                foreach (IPAddress address in
                    addresses.Result.Where(address => address.AddressFamily == AddressFamily.InterNetwork))
                {
                    ip = address;
                    break;
                }

                if (ip == null)
                {
                    Console.WriteLine("invalid host or wrong IP address found: " + hostNameOrAddress);
                    return result;
                }
            }

            try
            {
                List<Variable> vList = new List<Variable>();
                for (int i = 0; i < device.Oids.Count; i++)
                {
                    Variable test = new Variable(new ObjectIdentifier(device.Oids[i]));
                    vList.Add(test);
                }

                //List<Variable> vList = BuildVariables(DefaultOids);

                IPEndPoint receiver = new IPEndPoint(ip, port);
                if (version != VersionCode.V3)
                {
                    foreach (
                        Variable variable in
                            Messenger.Get(version, receiver, new OctetString(community), vList, timeout))
                    {
                        result.Add(variable);
                    }
                    return result;
                }

                if (string.IsNullOrEmpty(user))
                {
                    Console.WriteLine("User name need to be specified for v3.");
                    return result;
                }

                IAuthenticationProvider auth = (level & Levels.Authentication) == Levels.Authentication
                                                    ? GetAuthenticationProviderByName(authentication, authPhrase)
                                                    : DefaultAuthenticationProvider.Instance;

                IPrivacyProvider priv;
                if ((level & Levels.Privacy) == Levels.Privacy)
                {
                    priv = GetPrivacyProviderByName(privacy, privPhrase, auth);
                }
                else
                {
                    priv = new DefaultPrivacyProvider(auth);
                }

                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = discovery.GetResponse(timeout, receiver);

                GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(user), new OctetString(string.IsNullOrWhiteSpace(contextName) ? string.Empty : contextName), vList, priv, Messenger.MaxMessageSize, report);
                ISnmpMessage reply = request.GetResponse(timeout, receiver);
                if (reply is ReportMessage)
                {
                    if (reply.Pdu().Variables.Count == 0)
                    {
                        Console.WriteLine("wrong report message received");
                        return result;
                    }

                    var id = reply.Pdu().Variables[0].Id;
                    if (id != Messenger.NotInTimeWindow)
                    {
                        var error = id.GetErrorMessage();
                        Console.WriteLine(error);
                        return result;
                    }

                    // according to RFC 3414, send a second request to sync time.
                    request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(user), new OctetString(string.IsNullOrWhiteSpace(contextName) ? string.Empty : contextName), vList, priv, Messenger.MaxMessageSize, reply);
                    reply = request.GetResponse(timeout, receiver);
                }
                else if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
                {
                    throw ErrorException.Create(
                        "error in response",
                        receiver.Address,
                        reply);
                }

                foreach (Variable v in reply.Pdu().Variables)
                {
                    //Console.WriteLine(v);
                    result.Add(v);
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in GetAysn method of SNMP service=>{ex}");
                return result;     }

        }


        public async Task<List<Variable>> GetBulk(SnmpDevice device, int timeout = 5000)
        {
            List<Variable> result = new List<Variable>();
            string community = "public";
            bool showHelp = false;
            bool showVersion = false;
            VersionCode version = VersionCode.V1;
            int retry = 0;
            int maxRepetitions = 10;
            Levels level = Levels.Reportable;
            string user = string.Empty;
            string contextName = string.Empty;
            string authentication = string.Empty;
            string authPhrase = string.Empty;
            string privacy = string.Empty;
            string privPhrase = string.Empty;
            WalkMode mode = WalkMode.WithinSubtree;
            bool dump = false;

            IPAddress? ip =null;
            bool parsed = IPAddress.TryParse(device.IpAddress, out ip);
            if (!parsed)
            {
                var addresses = Dns.GetHostAddressesAsync(device.IpAddress);
                addresses.Wait();
                foreach (IPAddress address in
                    addresses.Result.Where(address => address.AddressFamily == AddressFamily.InterNetwork))
                {
                    ip = address;
                    break;
                }

                if (ip == null)
                {
                    Console.WriteLine("invalid host or wrong IP address found: " + device.IpAddress);
                    return result;
                }
            }

            try
            {
                ObjectIdentifier test = device.Oids.Count == 1 ? new ObjectIdentifier("1.3.6.1.2.1") : new ObjectIdentifier(device.IpAddress);
                IPEndPoint receiver = new IPEndPoint(ip, 161);
                if (version == VersionCode.V1)
                {
                    Messenger.Walk(version, receiver, new OctetString(community), test, result, timeout, mode);
                }
                else if (version == VersionCode.V2)
                {
                    Messenger.BulkWalk(version, receiver, new OctetString(community), new OctetString(string.IsNullOrWhiteSpace(contextName) ? string.Empty : contextName), test, result, timeout, maxRepetitions, mode, null, null);
                }
                else
                {
                    if (string.IsNullOrEmpty(user))
                    {
                        Console.WriteLine("User name need to be specified for v3.");
                        return result;
                    }

                    IAuthenticationProvider auth = (level & Levels.Authentication) == Levels.Authentication
                        ? GetAuthenticationProviderByName(authentication, authPhrase)
                        : DefaultAuthenticationProvider.Instance;
                    IPrivacyProvider priv;
                    if ((level & Levels.Privacy) == Levels.Privacy)
                    {
                        if (DESPrivacyProvider.IsSupported)
                        {
                            priv = new DESPrivacyProvider(new OctetString(privPhrase), auth);
                        }
                        else
                        {
                            Console.WriteLine("DES (ECB) is not supported by .NET Core.");
                            return result;
                        }
                    }
                    else
                    {
                        priv = new DefaultPrivacyProvider(auth);
                    }

                    Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetBulkRequestPdu);
                    ReportMessage report = discovery.GetResponse(timeout, receiver);
                    Messenger.BulkWalk(version, receiver, new OctetString(user), new OctetString(string.IsNullOrWhiteSpace(contextName) ? string.Empty : contextName), test, result, timeout, maxRepetitions, mode, priv, report);
                }
                return result;

            }
            catch (SnmpException ex)
            {
                Console.WriteLine(ex);
                return result;
            }

        }


        private IAuthenticationProvider GetAuthenticationProviderByName(string authentication, string phrase)
        {
            if (authentication.ToUpperInvariant() == "MD5")
            {
                return new MD5AuthenticationProvider(new OctetString(phrase));
            }

            if (authentication.ToUpperInvariant() == "SHA")
            {
                return new SHA1AuthenticationProvider(new OctetString(phrase));
            }

            if (authentication.ToUpperInvariant() == "SHA256")
            {
                return new SHA256AuthenticationProvider(new OctetString(phrase));
            }

            if (authentication.ToUpperInvariant() == "SHA384")
            {
                return new SHA384AuthenticationProvider(new OctetString(phrase));
            }

            if (authentication.ToUpperInvariant() == "SHA512")
            {
                return new SHA512AuthenticationProvider(new OctetString(phrase));
            }

            throw new ArgumentException("unknown name", nameof(authentication));
        }

        private IPrivacyProvider GetPrivacyProviderByName(string privacy, string phrase, IAuthenticationProvider auth)
        {
            if (string.IsNullOrEmpty(privacy))
            {
                return new DefaultPrivacyProvider(auth);
            }

            switch (privacy.ToUpperInvariant())
            {
                case "DES":
                    if (DESPrivacyProvider.IsSupported)
                    {
                        return new DESPrivacyProvider(new OctetString(phrase), auth);
                    }

                    throw new ArgumentException("DES privacy is not supported in this system");

                case "3DES":
                    return new TripleDESPrivacyProvider(new OctetString(phrase), auth);

                case "AES":
                    if (AESPrivacyProvider.IsSupported)
                    {
                        return new AESPrivacyProvider(new OctetString(phrase), auth); ;
                    }

                    throw new ArgumentException("AES privacy is not supported in this system");

                case "AES192":
                    if (AESPrivacyProvider.IsSupported)
                    {
                        return new AES192PrivacyProvider(new OctetString(phrase), auth);
                    }

                    throw new ArgumentException("AES192 privacy is not supported in this system");

                case "AES256":
                    if (AESPrivacyProvider.IsSupported)
                    {
                        return new AES256PrivacyProvider(new OctetString(phrase), auth);
                    }

                    throw new ArgumentException("AES256 privacy is not supported in this system");

                default:
                    throw new ArgumentException("unknown privacy name: " + privacy);
            }
        }
    }
}
