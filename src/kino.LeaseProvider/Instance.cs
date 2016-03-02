﻿using System;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public class Instance : IEquatable<Instance>
    {
        private readonly int hashCode;

        public Instance(string identity, TimeSpan maxAllowedLeaseTimeSpan)
        {
            MaxAllowedLeaseTimeSpan = maxAllowedLeaseTimeSpan;
            Identity = identity.GetBytes();
            hashCode = Identity?.GetHashCode() ?? 0;
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
            return Equals(Identity, other.Identity);
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
            => hashCode;

        public static bool operator ==(Instance left, Instance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Instance left, Instance right)
        {
            return !Equals(left, right);
        }

        public byte[] Identity { get; }

        public TimeSpan MaxAllowedLeaseTimeSpan { get; }
    }
}