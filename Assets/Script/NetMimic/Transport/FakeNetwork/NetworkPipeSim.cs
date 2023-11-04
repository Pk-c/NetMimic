#nullable enable

using System.Collections.Generic;

namespace NetMimic
{
    public static class NetworkPipeSim
    {
        public static FakeNetworkTransport? Host = null;
        public static List<FakeNetworkTransport> Client = new List<FakeNetworkTransport>();
    }
}
