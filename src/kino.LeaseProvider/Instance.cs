using System;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public class Instance : IEquatable<Instance>
    {
        private readonly string identity;
        private readonly byte[] byteIdentity;

        public Instance(string identity)
        {
            this.identity = identity;
            byteIdentity = identity.GetBytes();
        }

        public bool Equals(Instance other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(identity, other.identity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Instance) obj);
        }

        public override int GetHashCode()
        {
            return identity?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Instance left, Instance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Instance left, Instance right)
        {
            return !Equals(left, right);
        }

        public string GetStringIdentity()
            => identity;

        public byte[] GetIdentity()
            => byteIdentity;
    }
}