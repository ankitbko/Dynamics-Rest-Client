namespace Microsoft.Dynamics.CrmRestClient
{
	using System;
	using System.Threading;

	internal class LockInfo
	{
		private const int LockValue = 1;
		private const int UnlockValue = 0;

		private TimeSpan acquireTimeout = TimeSpan.FromHours(1);
		public TimeSpan AcquireTimeout
		{
			get
			{
				return this.acquireTimeout;
			}
			set
			{
				this.acquireTimeout = value.WithinBoundaries(minimum: TimeSpan.FromSeconds(30), maximum: TimeSpan.FromDays(1));
			}
		}

		private TimeSpan releaseTimeout = TimeSpan.FromDays(1);
		public TimeSpan ReleaseTimeout
		{
			get
			{
				return this.releaseTimeout;
			}
			set
			{
				this.releaseTimeout = value.WithinBoundaries(minimum: TimeSpan.FromMinutes(1), maximum: TimeSpan.FromDays(2));
			}
		}

		private int value = LockInfo.UnlockValue;

		private Guid currentLock = Guid.Empty;

		private DateTime lockAcquiredOn = DateTime.UtcNow;

		public LockInfo(TimeSpan? acquireTimeout = null, TimeSpan? releaseTimeout = null)
		{
			this.lockAcquiredOn = DateTime.UtcNow;
			this.AcquireTimeout = (acquireTimeout ?? TimeSpan.FromHours(1));
			this.ReleaseTimeout = (releaseTimeout ?? TimeSpan.FromDays(1));
		}

		public Guid AcquireLock()
		{
			var lockKey = Guid.NewGuid();
			var startTrying = DateTime.UtcNow;
			while (!this.TryAcquireLock())
			{
				if (DateTime.UtcNow - startTrying > this.AcquireTimeout)
				{
					this.Throw("The object timed out while trying to acquire a lock.", forceReleaseLock: true);
				}
				else if (DateTime.UtcNow - this.lockAcquiredOn > this.ReleaseTimeout && this.currentLock != Guid.Empty)
				{
					this.Throw("The object had been locked for too long.", forceReleaseLock: true);
				}
			}
			this.currentLock = lockKey;
			this.lockAcquiredOn = DateTime.UtcNow;
			return lockKey;
		}

		public bool TryAcquireLock(out Guid lockKey)
		{
			if (this.TryAcquireLock())
			{
				lockKey = Guid.NewGuid();
				this.currentLock = lockKey;
				this.lockAcquiredOn = DateTime.UtcNow;
				return true;
			}
			lockKey = Guid.Empty;
			return false;
		}

		public void ProceedWhenUnlocked()
		{
			while (this.value.Equals(LockInfo.LockValue))
			{
				if (DateTime.UtcNow - this.lockAcquiredOn > this.ReleaseTimeout && this.currentLock != Guid.Empty)
				{
					this.Throw("The object had been locked for too long.", forceReleaseLock: true);
				}
			}
		}

		public bool ReleaseLock(Guid lockKey)
		{
			if (this.currentLock.Equals(lockKey) || this.currentLock.Equals(Guid.Empty))
			{
				while (this.value.Equals(LockInfo.LockValue))
				{
					if (!this.TryReleaseLock() && DateTime.UtcNow - this.lockAcquiredOn > this.ReleaseTimeout && this.currentLock != Guid.Empty)
					{
						this.Throw("The object had been locked for too long.", forceReleaseLock: true);
					}
				}
				this.currentLock = Guid.Empty;
				return true;
			}
			else
			{
				this.Throw("The lock couldn't be released because it wasn't acquired using the provided key.", forceReleaseLock: false);
			}
			return false;
		}

		private void Throw(string message, bool forceReleaseLock = true)
		{
			if (forceReleaseLock)
			{
				this.currentLock = Guid.Empty;
				this.value = LockInfo.UnlockValue;
			}
			throw new LockableException(message);
		}

		private bool TryAcquireLock()
		{
			return Interlocked.CompareExchange(ref this.value, LockInfo.LockValue, LockInfo.UnlockValue).Equals(LockInfo.UnlockValue);
		}

		private bool TryReleaseLock()
		{
			return Interlocked.CompareExchange(ref this.value, LockInfo.UnlockValue, LockInfo.LockValue).Equals(LockInfo.LockValue);
		}
	}
	
}
