﻿using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow;
using Mediachase.Commerce.Workflow.Cart;
using Mediachase.Commerce.Workflow.OrderGroupActivities;
using Mediachase.Commerce.Workflow.PurchaseOrderActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// Activity flow that validates an order. 
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartValidateWorkflowName, AvailableInBetaMode = true)]
    public class CartValidateVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<ValidateLineItemsActivity>()
                    .Do<GetFulfillmentWarehouseActivity>()
                    .If(ShouldCheckInstoreInventory)
                        .Do<CheckInstoreInventoryActivity>()
                    .Else()
                        .Do<CheckInventoryActivity>()
                    .EndIf()
                    .Do<RemoveDiscountsVNextActivity>()
                    .Do<CalculateDiscountsVNextActivity>()
                    .Do<UpdateTotalsVNextActivity>();
        }
    }
}
