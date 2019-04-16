namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Timers;

	public static class BasicExtensions
	{
		public const int DefaultMaxDegreeOfParallelism = 25;

		/// <summary>
		/// Invokes a given function after converting a given object if it matches a certain type and it's not null. This method doesn't raise exceptions.
		/// </summary>
		/// <typeparam name="TInvoke">Type to match the object to.</typeparam>
		/// <param name="invokable">Object to be sent as parameter.</param>
		/// <param name="function">Function to be invoked.</param>
		/// <returns>Task for asynchronous behavior.</returns>
		internal static Task InvokeAs<TInvoke>(this object invokable, Func<TInvoke, Task> function)
		{
			try
			{
				return invokable.RunIfNotNull(
					input =>
					{
						if (invokable is TInvoke)
						{
							var newInvokable = (TInvoke)invokable;
							return function.Invoke(newInvokable);
						}
						return Task.FromResult<object>(null);
					},
					Task.FromResult<object>(null));
			}
			catch
			{
				return Task.FromResult<object>(null);
			}
		}
        
		internal static Task<TResult> InvokeAs<TInvoke, TResult>(this object invokable, Func<TInvoke, Task<TResult>> function)
		{
			try
			{
				return invokable.RunIfNotNull(
					input =>
					{
						if (invokable is TInvoke)
						{
							var newInvokable = (TInvoke)invokable;
							return function.Invoke(newInvokable);
						}
						return Task.FromResult(default(TResult));
					},
					Task.FromResult(default(TResult)));
			}
			catch
			{
				return Task.FromResult(default(TResult));
			}
		}
        
		internal static Func<TParameter, Task> ToAsyncFunction<TParameter>(this Action<TParameter> action)
		{
			return 
				(parameter) => 
				{
					action.Invoke(parameter);
					return Task.FromResult<object>(null);
				};
		}
        
		internal static Func<TParameter, Task<TResult>> ToAsyncFunction<TParameter, TResult>(this Action<TParameter> action)
		{
			return 
				(parameter) => 
				{
					action.Invoke(parameter);
					return Task.FromResult(default(TResult));
				};
		}
        
		internal static Task InvokeAs<TInvoke>(this object invokable, Action<TInvoke> action)
		{
			try
			{
				return invokable.InvokeAs(action.ToAsyncFunction());
			}
			catch
			{
				return Task.FromResult<object>(null);
			}
		}
        
		internal static Task<TResult> InvokeAs<TInvoke, TResult>(this object invokable, Action<TInvoke> action)
		{
			try
			{
				return invokable.InvokeAs(action.ToAsyncFunction<TInvoke, TResult>());
			}
			catch
			{
				return Task.FromResult(default(TResult));
			}
		}
        
		internal static void Destroy(this object destroyable)
		{
			try
			{
				if (destroyable != null)
				{
					destroyable.InvokeAs<byte[]>(input => input.RunPerItem(item => { item = 0; }));
					destroyable.InvokeAs<Timer>(input => input.Stop());
					destroyable.InvokeAs<StreamWriter>(input => input.Close());
					destroyable.InvokeAs<StreamReader>(input => input.Close());
					destroyable.InvokeAs<Stream>(input => input.Close());
					destroyable.InvokeAs<CryptoStream>(input => input.Close());
					destroyable.InvokeAs<IList>(input => input.Clear());
					destroyable.InvokeAs<IDictionary>(input => input.Clear());
					destroyable.InvokeAs<IDbConnection>(input => input.Close());
					destroyable.InvokeAs<IDataReader>(input => input.Close());
					destroyable.InvokeAs<IDisposable>(input => input.Dispose());
					destroyable = null;
				}
			}
			catch { }
		}
        
		internal static void RunPerItem<TItem>(this TItem[] items, Action<TItem> action, int maxDegreeOfParallelism = 1)
		{
			Parallel.For(0, items.Length, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism.WithinBoundaries(minimum: 1) }, index => action.Invoke(items[index]));
		}
        
		internal static void RunPerItem<TItem>(this IEnumerable<TItem> items, Action<TItem> action, int maxDegreeOfParallelism = 1)
		{
			Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism.WithinBoundaries(minimum: 1) }, item => action.Invoke(item));
		}

		internal static string ToCleanString(this Guid guid)
		{
			return guid.ToString().Replace("{", string.Empty).Replace("}", string.Empty).Replace(" ", string.Empty);
		}

		internal static MemoryStream ToStream(this string value, Encoding encoding = null)
		{
			return new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(value ?? string.Empty));
		}

        internal static MemoryStream ToStream(this ArraySegment<byte> value)
		{
			return new MemoryStream(value.Array, value.Offset, value.Count);
		}

		internal static TItem[] ToArray<TItem>(this ArraySegment<TItem> segment)
		{
			return segment.ToList().ToArray();
		}

		public static TResult AsType<TResult>(this object value, TResult defaultValue = default(TResult), CultureInfo cultureInfo = null)
		{
			try
			{
                if (value == null)
                    return defaultValue;

				return (TResult)Convert.ChangeType(value, typeof(TResult), cultureInfo ?? CultureInfo.InvariantCulture);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static TResult ReadChildAs<TResult>(this JToken sourceJson, string tokenName, TResult defaultValue = default(TResult))
		{
			try
			{
                var value = sourceJson[tokenName];
                if (value == null)
                    return defaultValue;

                return value.AsType<TResult>();
			}
			catch
			{
				return defaultValue;
			}
		}

        internal static TResult RunIfNotNull<TInput, TResult>(this TInput input, Func<TInput, TResult> function, TResult defaultResult = default(TResult)) where TInput : class
		{
			try
			{
				return input == null ? defaultResult : function.Invoke(input);
			}
			catch
			{
				return defaultResult;
			}
		}

        internal static TResult RunIfNotNull<TInput, TResult>(this Nullable<TInput> input, Func<Nullable<TInput>, TResult> function, TResult defaultResult = default(TResult)) where TInput : struct
		{
			try
			{
				return input == null ? defaultResult : function.Invoke(input);
			}
			catch
			{
				return defaultResult;
			}
		}

        internal static int WithinBoundaries(this int value, int minimum = int.MinValue, int maximum = int.MaxValue)
		{
			return value < minimum ? minimum : (value > maximum ? maximum : value);
		}

        internal static long WithinBoundaries(this long value, long minimum = long.MinValue, long maximum = long.MaxValue)
		{
			return value < minimum ? minimum : (value > maximum ? maximum : value);
		}

        internal static decimal WithinBoundaries(this decimal value, decimal minimum = decimal.MinValue, decimal maximum = decimal.MaxValue)
		{
			return value < minimum ? minimum : (value > maximum ? maximum : value);
		}

        internal static double WithinBoundaries(this double value, double minimum = double.MinValue, double maximum = double.MaxValue)
		{
			return value < minimum ? minimum : (value > maximum ? maximum : value);
		}

        internal static DateTime WithinBoundaries(this DateTime value, DateTime? minimum = null, DateTime? maximum = null)
		{
			return value < (minimum ?? DateTime.MinValue) ? (minimum ?? DateTime.MinValue) : (value > (maximum ?? DateTime.MaxValue) ? (maximum ?? DateTime.MaxValue) : value);
		}

        internal static TimeSpan WithinBoundaries(this TimeSpan value, TimeSpan? minimum = null, TimeSpan? maximum = null)
		{
			return value < (minimum ?? TimeSpan.MinValue) ? (minimum ?? TimeSpan.MinValue) : (value > (maximum ?? TimeSpan.MaxValue) ? (maximum ?? TimeSpan.MaxValue) : value);
		}

        /// <summary>
        /// Get all field names from the JSON.
        /// </summary>
        /// <param name="sourceJson">JSON to read fields from.</param>
        /// <returns>Collection of field names</returns>
        public static List<string> GetChildNames(this JToken sourceJson)
        {
            return (sourceJson as JObject)
                ?.Properties()
                ?.Select(property => property?.Name).ToList()
                ?? new List<string>();
        }

        public static bool IsJSON(this string jsonString)
        {
            try
            {
                Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
