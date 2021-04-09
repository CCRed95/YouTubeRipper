using System;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace YouTubeRipper.Media.Metadata
{
	public abstract class ShellRefProperty<_TValue>
	{
		protected PropertyKey PropertyKey;
		

		public abstract _TValue GetValueBase(
			ShellRefObject parentBase);

		public abstract void SetValueBase(
			ShellRefObject parentBase,
			_TValue value);


		internal class ShellRefPropertyImpl<TParent, TValue>
			: ShellRefProperty<TValue>
				where TParent
					: ShellRefObject
		{
			private ShellProperty<TValue> _shellProperty;


			public ShellRefPropertyImpl(
				PropertyKey propertyKey) 
					: base(propertyKey)
			{
			}


			protected ShellProperty<TValue> GetShellProperty(
				ShellRefObject parent)
			{
				return _shellProperty ??= parent
					.ShellObject
					.Properties
					.GetProperty<TValue>(PropertyKey);
			}


			public override TValue GetValueBase(
				ShellRefObject parentBase)
			{
				return GetValue((TParent) parentBase);
			}

			public override void SetValueBase(
				ShellRefObject parentBase,
				TValue value)
			{
				SetValue((TParent)parentBase, value);
			}


			public TValue GetValue(
				TParent parent)
			{
				return GetShellProperty(parent).Value;
			}
			
			public void SetValue(
				TParent parent,
				TValue value)
			{
				GetShellProperty(parent).Value = value;
			}
		}

		internal class ShellRefPropertyComplexImpl<TParent, TRawValue, TValue>
			: ShellRefPropertyImpl<TParent, TValue>
				where TParent : ShellRefObject
		{
			private ShellProperty<TRawValue> _shellProperty;
			private readonly Func<TRawValue, TValue> _convertTo;
			private readonly Func<TValue, TRawValue> _convertFrom;


			protected new ShellProperty<TRawValue> GetShellProperty(
				ShellRefObject parent)
			{
				return _shellProperty ??= parent
					.ShellObject
					.Properties
					.GetProperty<TRawValue>(PropertyKey);
			}
			
			public override TValue GetValueBase(
				ShellRefObject parentBase)
			{
				return GetValue((TParent)parentBase);
			}

			public override void SetValueBase(
				ShellRefObject parentBase,
				TValue value)
			{
				SetValue((TParent)parentBase, value);
			}


			public new TValue GetValue(
				TParent parent)
			{
				var rawValue = GetShellProperty(parent).Value;
				return _convertTo(rawValue);
			}

			public new void SetValue(
				TParent parent,
				TValue value)
			{
				var rawValue = _convertFrom(value);
				GetShellProperty(parent).Value = rawValue;
			}


			public ShellRefPropertyComplexImpl(
				PropertyKey propertyKey,
				Func<TRawValue, TValue> convertTo,
				Func<TValue, TRawValue> convertFrom) : base(
					propertyKey)
			{
				_convertTo = convertTo;
				_convertFrom = convertFrom;
			}
		}

		
		protected ShellRefProperty(
			PropertyKey propertyKey)
		{
			PropertyKey = propertyKey;
		}
	}
}


/*		public object ValueBase
		{
			get => _shellPropertyBase.ValueAsObject;
			set => _shellPropertyBase. = value;
		}*/
//public TValue GetValue(
//	TParent parent)
//{
//	return _shellProperty.Value;
//}
//public void SetValue(
//	TParent parent,
//	TValue value)
//{
//	_shellProperty.Value = value;
//}