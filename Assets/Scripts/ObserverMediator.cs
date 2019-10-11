using System;
using System.Collections.Generic;

/// <summary>
/// convenience implementation for observables
/// </summary>
/// <typeparam name="T"> type to be observed </typeparam>
public class ObserverMediator<T> : IObservable<T>, IObserver<T> {

    private readonly List<IObserver<T>> observers = new List<IObserver<T>>();

    public void OnCompleted() {
        foreach(var o in observers) {
            o.OnCompleted();
        }
    }

    public void OnError(Exception error) {
        foreach(var o in observers) {
            o.OnError(error);
        }
    }

    public void OnNext(T value) {
        foreach(var o in observers) {
            o.OnNext(value);
        }
    }

    public IDisposable Subscribe(IObserver<T> observer) {
        if(!observers.Contains(observer)) {
            observers.Add(observer);
        }
        return new Unsubscriber(observers, observer);
    }

    private class Unsubscriber : IDisposable {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose() {
            if(_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
