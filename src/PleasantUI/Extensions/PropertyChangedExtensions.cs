﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace PleasantUI.Extensions;

public static class PropertyChangedExtensions
{
    public static IObservable<TRes> WhenAnyValue<TModel, TRes>(this TModel model,
        Expression<Func<TModel, TRes>> expr) where TModel : INotifyPropertyChanged
    {
        LambdaExpression l = expr;
        MemberExpression ma = (MemberExpression)l.Body;
        PropertyInfo prop = (PropertyInfo)ma.Member;
        return new PropertyObservable<TRes>(model, prop);
    }

    public static IObservable<TRes> WhenAnyValue<TModel, T1, TRes>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Func<T1, TRes> cb
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1).Select(cb);
    }

    public static IObservable<TRes> WhenAnyValue<TModel, T1, T2, TRes>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2,
        Func<T1, T2, TRes> cb
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1).CombineLatest(model.WhenAnyValue(v2),
            cb);
    }

    public static IObservable<ValueTuple<T1, T2>> WhenAnyValue<TModel, T1, T2>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1, v2, (a1, a2) => (a1, a2));
    }

    public static IObservable<TRes> WhenAnyValue<TModel, T1, T2, T3, TRes>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2,
        Expression<Func<TModel, T3>> v3,
        Func<T1, T2, T3, TRes> cb
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1).CombineLatest(model.WhenAnyValue(v2),
            model.WhenAnyValue(v3),
            cb);
    }

    public static IObservable<ValueTuple<T1, T2, T3>> WhenAnyValue<TModel, T1, T2, T3>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2,
        Expression<Func<TModel, T3>> v3
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1, v2, v3, (a1, a2, a3) => (a1, a2, a3));
    }

    private class PropertyObservable<T> : IObservable<T>
    {
        private readonly PropertyInfo _info;
        private readonly INotifyPropertyChanged _target;

        public PropertyObservable(INotifyPropertyChanged target, PropertyInfo info)
        {
            _target = target;
            _info = info;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return new Subscription(_target, _info, observer);
        }

        private class Subscription : IDisposable
        {
            private readonly PropertyInfo _info;
            private readonly IObserver<T> _observer;
            private readonly INotifyPropertyChanged _target;

            public Subscription(INotifyPropertyChanged target, PropertyInfo info, IObserver<T> observer)
            {
                _target = target;
                _info = info;
                _observer = observer;
                _target.PropertyChanged += OnPropertyChanged;
                _observer.OnNext((T)_info.GetValue(_target)!);
            }

            public void Dispose()
            {
                _target.PropertyChanged -= OnPropertyChanged;
                _observer.OnCompleted();
            }

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _info.Name)
                    _observer.OnNext((T)_info.GetValue(_target)!);
            }
        }
    }
}