using System;

namespace iSukces.Binding
{
    public class FakePropertyInfo : IPropertyInfo 
    {
        public IPropertyAccessor2 Create(object obj) { return new FakePropertyAccessor(); }

        public int             Priority => int.MinValue;
        public BindingFeatures Features => BindingFeatures.None;

        private class FakePropertyAccessor : DisposableBase, IPropertyAccessor2
        {
            public FakePropertyAccessor()
                : base(DisposingStateBehaviour.None)
            {
            }

            public IDisposable SubscribePropertyNotification(UpdateSourceDe updateBackAction) { return new FakePropertySession(); }

            public UpdateSourceResult PropertySetValue(object value)
            {
                return UpdateSourceResult.NotSet;
                ;
            }


            public Type PropertyType { get; set; }
        }

        private class FakePropertySession : DisposableBase, IDisposable
        {
            public FakePropertySession()
                : base(DisposingStateBehaviour.None)
            {
            }
        }
    }
}
