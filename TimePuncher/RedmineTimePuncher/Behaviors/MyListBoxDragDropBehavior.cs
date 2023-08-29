using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.DragDrop.Behaviors;

namespace RedmineTimePuncher.Behaviors
{
    public class MyListBoxDragDropBehavior : ListBoxDragDropBehavior
    {
        public override void DragDropCompleted(DragDropState state)
        {
            // In order to avoid the removal of the items from the ListBox,
            // when an item is dropped we shouldn't call the base method.
            //
            // base.DragDropCompleted(state);
        }

        public override bool CanDrop(DragDropState state)
        {
            // Don't want to drop on the ListBox at all.
            return false;
        }

        protected override bool IsMovingItems(DragDropState state)
        {
            return false;
        }
    }
}
