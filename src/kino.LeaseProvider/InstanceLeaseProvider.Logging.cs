﻿using System;
using kino.Consensus;

namespace kino.LeaseProvider
{
    public partial class InstanceLeaseProvider
    {
        private const string format = "HH:mm:ss fff";

        private void LogAwake()
        {
            logger.Debug($"SLEEP === process {localNode.Uri.AbsoluteUri} " +
                         $"Waked up at {DateTime.UtcNow.ToString(format)}");
        }

        private void LogStartSleep()
        {
            logger.Debug($"SLEEP === process {localNode.Uri.AbsoluteUri} " +
                         $"Sleep from {DateTime.UtcNow.ToString(format)}");
        }

        private void LogLeaseProlonged(byte[] requestorIdentity, Lease lastReadLease)
        {
            if (lastReadLease != null)
            {
                if (IsLeaseOwner(requestorIdentity, lastReadLease))
                {
                    logger.Debug($"[{DateTime.UtcNow.ToString(format)}] " +
                                 "PROLONG === process " +
                                 $"{localNode.Uri.AbsoluteUri} " +
                                 "wants to prolong it's lease " +
                                 $"{lastReadLease.ExpiresAt.ToString(format)}");
                }
                else
                {
                    logger.Debug($"[{DateTime.UtcNow.ToString(format)}] " +
                                 "RENEW === process " +
                                 $"{localNode.Uri.AbsoluteUri} " +
                                 "wants to renew lease " +
                                 $"{lastReadLease.ExpiresAt.ToString(format)}");
                }
            }
        }
    }
}