using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class INotifyPropertyChangedExtensions
    {
        public static ReadOnlyReactivePropertySlim<TViewModel> ToReadOnlyViewModel<TModel, TViewModel, TModelProperty>(
            this TModel model,
            Expression<Func<TModel, TModelProperty>> propertyExpression,
            Func<TModelProperty, TViewModel> createViewModel)
            where TModel : INotifyPropertyChanged
        {
            return model.ObserveProperty(propertyExpression)
                .Select(createViewModel)
                .DisposePreviousValue()
                .ToReadOnlyReactivePropertySlim();
        }
    }
}
