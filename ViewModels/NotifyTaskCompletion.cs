using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ccr.Std.Core.Extensions;

namespace YouTubeRipper.ViewModels
{
	public sealed class NotifyTaskCompletion<TResult>
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;


		public Task<TResult> Task { get; }

		public TResult Result
		{
			get => Task.Status == TaskStatus.RanToCompletion
				? Task.Result
				: default;
		}

		public TaskStatus Status
		{
			get => Task.Status;
		}

		public bool IsCompleted
		{
			get => Task.IsCompleted;
		}

		public bool IsNotCompleted
		{
			get => !Task.IsCompleted;
		}

		public bool IsSuccessfullyCompleted
		{
			get => Task.Status == TaskStatus.RanToCompletion;
		}

		public bool IsCanceled
		{
			get => Task.IsCanceled;
		}

		public bool IsFaulted
		{
			get => Task.IsFaulted;
		}

		public AggregateException Exception
		{
			get => Task.Exception;
		}

		public Exception InnerException
		{
			get => Exception?.InnerException;
		}

		public string ErrorMessage
		{
			get => InnerException?.Message;
		}


		public NotifyTaskCompletion(
			Task<TResult> task)
		{
			Task = task;

			if (!task.IsCompleted)
			{
				var _ = WatchTaskAsync(task);
			}
		}


		private void NotifyOfPropertyChange<TProperty>(
			Expression<Func<TProperty>> property)
		{
			var propertyName = property.GetMemberInfo().Name;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async Task WatchTaskAsync(Task task)
		{
			try
			{
				await task;
			}
			catch
			{
			}

			//var propertyChanged = PropertyChanged;
			//if (propertyChanged == null)
			//	return;

			NotifyOfPropertyChange(() => Status);
			NotifyOfPropertyChange(() => IsCompleted);
			NotifyOfPropertyChange(() => IsNotCompleted);

			if (task.IsCanceled)
			{
				NotifyOfPropertyChange(() => IsCanceled);
			}
			else if (task.IsFaulted)
			{
				NotifyOfPropertyChange(() => IsFaulted);
				NotifyOfPropertyChange(() => Exception);
				NotifyOfPropertyChange(() => InnerException);
				NotifyOfPropertyChange(() => ErrorMessage);
			}
			else
			{
				NotifyOfPropertyChange(() => IsSuccessfullyCompleted);
				NotifyOfPropertyChange(() => Result);
			}
		}
	}
}