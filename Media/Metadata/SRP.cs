using System;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace YouTubeRipper.Media.Metadata
{
	public static class SRP
	{
		public static ShellRefProperty<TValue> Register<TParent, TValue>(
			PropertyKey propertyKey)
			where TParent
				: ShellRefObject
		{
			return new ShellRefProperty<TValue>
				.ShellRefPropertyImpl<TParent, TValue>(
					propertyKey);
		}

		public static ShellRefProperty<TValue> Register<TParent, TRawValue, TValue>(
			PropertyKey propertyKey,
			Func<TRawValue, TValue> convertTo,
			Func<TValue, TRawValue> convertFrom)
			where TParent
				: ShellRefObject
		{
			return new ShellRefProperty<TValue>
				.ShellRefPropertyComplexImpl<TParent, TRawValue, TValue>(
					propertyKey,
					convertTo,
					convertFrom);
		}
	}
}