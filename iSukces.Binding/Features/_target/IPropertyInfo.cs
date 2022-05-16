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
        ///     There is dedicated event to track property changes i.e. VisibleChanged
        /// </summary>
        SpecialEvent = 2,


        BindOnPropertyChanged = 4,

        BindOnLostFocus = 8,

        TwoWayBindingByDefault = 16,

        /// <summary>
        /// Property is default property for control i.e. Text for TextBox
        /// </summary>
        DefaultControlProperty = 32,

        InvalidValueNotification = 64
    }
}
