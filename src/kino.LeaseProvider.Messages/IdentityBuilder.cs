using kino.Core.Framework;

namespace kino.LeaseProvider.Messages
{
    internal static class IdentityBuilder
    {
        internal static byte[] BuildFullIdentity(this string identity)
            => ("LP." + identity).GetBytes();
    }
}