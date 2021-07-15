namespace Compus.Gateway
{
    internal class GatewayPayload<TData>
    {
        public GatewayOpcode Op { get; init; }

        public TData? D { get; init; }

        public int? S { get; init; }

        public string? T { get; init; }
    }
}
