namespace Microsoft.Dynamics.CrmRestClient
{
	using System;
	using System.Collections.Concurrent;
	using System.Runtime.CompilerServices;

	public static class LockableExtensions
	{
		private static readonly ConditionalWeakTable<ILockable, ConcurrentDictionary<string, LockInfo>> lockables = new ConditionalWeakTable<ILockable, ConcurrentDictionary<string, LockInfo>>();

		public static Guid AcquireLock(this ILockable lockable, string resource = default(string))
		{
			var lockInfo = LockableExtensions.lockables.GetOrCreateValue(lockable).GetOrAdd(resource ?? string.Empty, new LockInfo());
			return lockInfo.AcquireLock();
		}

		public static bool TryAcquireLock(this ILockable lockable, out Guid lockKey, string resource = default(string))
		{
			var lockInfo = LockableExtensions.lockables.GetOrCreateValue(lockable).GetOrAdd(resource ?? string.Empty, new LockInfo());
			return lockInfo.TryAcquireLock(out lockKey);
		}

		public static void ProceedWhenUnlocked(this ILockable lockable, string resource = default(string))
		{
			var lockInfo = LockableExtensions.lockables.GetOrCreateValue(lockable).GetOrAdd(resource ?? string.Empty, new LockInfo());
			lockInfo.ProceedWhenUnlocked();
		}

		public static bool ReleaseLock(this ILockable lockable, Guid lockKey, string resource = default(string))
		{
			var lockInfo = LockableExtensions.lockables.GetOrCreateValue(lockable).GetOrAdd(resource ?? string.Empty, new LockInfo());
			return lockInfo.ReleaseLock(lockKey);
		}
	}
}
