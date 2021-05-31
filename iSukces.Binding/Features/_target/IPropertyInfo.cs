using System;

namespace iSukces.Binding
{
    public interface IPropertyInfo
    {
        int             Priority { get; }
        BindingFeatures Features { get; }

        IPropertyAccessor2 Create(object obj);
    }


    [Flags]
    public enum BindingFeatures
    {
        None = 0,

        /// <summary>
        ///     Target object supports OnNotifyPropertyChanged
        /// </summary>
        OnPropertyChanged = 1,

        /// <summary>
        ///     There is dedicated event to track property changes i.e. OnVisibleChanged
        /// </summary>
        SpecialEvent = 2,


        BindOnPropertyChanged = 4,
        
        BindOnLostFocus = 8,
        
        TwoWayBindingByDefault = 16
    }
}
