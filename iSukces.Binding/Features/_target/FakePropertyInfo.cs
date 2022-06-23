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

            public void ForceUpdateSourceDELE() { }

            public UpdateSourceResult PropertySetValue(object value)
            {
                return UpdateSourceResult.NotSet;
                ;
            }

            public IPropertyUpdateSession SubscribePropertyNotification(UpdateSourceDe updateBackAction)
            {
                return new FakePropertySession();
            }


            public Type PropertyType { get; set; }
        }

        private class FakePropertySession : DisposableBase, IPropertyUpdateSession
        {
            public FakePropertySession()
                : base(DisposingStateBehaviour.None)
            {
            }

            public void ForceUpdate(bool ignoreDisabledControls)
            {
            }
        }
    }
}
